using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor2D))]
public class NpcBrain2D : MonoBehaviour
{
    public enum NpcMode { Idle, Patrol, Wander, Follow, Manual }

    [System.Serializable]
    public class WanderSettings
    {
        public enum WanderPattern { WalkThenIdle, IdleThenWalk }

        public WanderPattern pattern = WanderPattern.WalkThenIdle;
        public Vector2 wanderIntervalRange = new Vector2(1.2f, 2.0f);
        public Vector2 idleIntervalRange = new Vector2(0.5f, 1.5f);
        public float wanderTurnSpeed = 4f;
    }

    [System.Serializable]
    public class PatrolSettings
    {
        public enum PatrolPattern { WalkThenIdle, IdleThenWalk }

        public PatrolPattern pattern = PatrolPattern.WalkThenIdle;
        public float waypointReachDistance = 0.1f;
        public float idleTimeAtWaypoint = 0.5f;
        public float initialIdleTime = 1.0f; // For IdleThenWalk pattern
    }

    [Header("Mode")]
    public NpcMode mode = NpcMode.Patrol;

    [Header("Patrol")]
    public Transform[] waypoints;
    public PatrolSettings patrolSettings = new PatrolSettings();

    [Header("Wander")]
    public WanderSettings wanderSettings = new WanderSettings();

    [Header("Follow")]
    public Transform followTarget;
    public float chaseDistance = 4f;
    public float stopDistance = 0.75f;

    private CharacterMotor2D _motor;
    private int _currentWp;
    private Vector2 _desiredMove;
    private Coroutine _wanderRoutine;
    private Coroutine _patrolRoutine;
    private bool _isPatrolIdling;

    void Awake() => _motor = GetComponent<CharacterMotor2D>();

    void OnEnable()
    {
        if (mode == NpcMode.Wander)
            _wanderRoutine = StartCoroutine(WanderLoop());
        else if (mode == NpcMode.Patrol)
            StartPatrol();
    }

    void OnDisable()
    {
        if (_wanderRoutine != null) StopCoroutine(_wanderRoutine);
        if (_patrolRoutine != null) StopCoroutine(_patrolRoutine);
    }

    void Update()
    {
        if (_motor.IsDialogueActive || _motor.IsTeleporting || ClockTimer.IsTimeEnded || GlobalPause.IsMinigamePaused)
        {
            _motor.SetMoveInput(Vector2.zero);
            return;
        }

        switch (mode)
        {
            case NpcMode.Idle:
                _desiredMove = Vector2.zero;
                break;

            case NpcMode.Patrol:
                PatrolTick();
                break;

            case NpcMode.Follow:
                FollowTick();
                break;

            case NpcMode.Wander:
                // _desiredMove is set by WanderLoop; we may also switch to Follow if close enough
                if (followTarget != null && Vector2.Distance(transform.position, followTarget.position) < chaseDistance)
                {
                    FollowTick();
                }
                break;
        }

        // Only update motor input if not paused (we already returned early above if paused)
        _motor.SetMoveInput(_desiredMove);
    }

    public IEnumerator MoveToPosition(Vector2 target)
    {
        if (_motor.IsDialogueActive || _motor.IsTeleporting)
            yield break;

        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            // Check pause conditions in the loop
            if (_motor.IsDialogueActive || _motor.IsTeleporting || ClockTimer.IsTimeEnded || GlobalPause.IsMinigamePaused)
            {
                _desiredMove = Vector2.zero;
                yield return null;
                continue;
            }
            
            var to = (target - (Vector2)transform.position).normalized;
            _desiredMove = to;
            yield return null;
        }

        _desiredMove = Vector2.zero;
    }

    private void StartPatrol()
    {
        if (_patrolRoutine != null) StopCoroutine(_patrolRoutine);

        if (patrolSettings.pattern == PatrolSettings.PatrolPattern.IdleThenWalk)
        {
            _patrolRoutine = StartCoroutine(PatrolIdleThenWalk());
        }
        else
        {
            _patrolRoutine = StartCoroutine(PatrolWalkThenIdle());
        }
    }

    private void PatrolTick()
    {
        // For IdleThenWalk pattern, movement is handled in coroutine
        if (patrolSettings.pattern == PatrolSettings.PatrolPattern.IdleThenWalk)
        {
            if (_isPatrolIdling)
            {
                _desiredMove = Vector2.zero;
            }
            else
            {
                // Move towards current waypoint
                if (waypoints == null || waypoints.Length == 0)
                {
                    _desiredMove = Vector2.zero;
                    return;
                }

                var target = waypoints[_currentWp].position;
                var to = (Vector2)(target - transform.position);
                _desiredMove = to.normalized;
            }
        }
        else
        {
            // For WalkThenIdle pattern, use the original logic
            if (waypoints == null || waypoints.Length == 0)
            {
                _desiredMove = Vector2.zero;
                return;
            }

            var target = waypoints[_currentWp].position;
            var to = (Vector2)(target - transform.position);
            if (to.magnitude <= patrolSettings.waypointReachDistance)
            {
                _desiredMove = Vector2.zero;
                if (_patrolRoutine == null) // Only start coroutine if not already running
                {
                    _patrolRoutine = StartCoroutine(AdvanceAfterPause());
                }
            }
            else
            {
                _desiredMove = to.normalized;
            }
        }
    }

    private IEnumerator PatrolWalkThenIdle()
    {
        // Original behavior: walk to waypoint, idle, then move to next
        while (true)
        {
            // Wait until we reach the waypoint (this is checked in PatrolTick)
            while (Vector2.Distance(transform.position, waypoints[_currentWp].position) > patrolSettings.waypointReachDistance)
            {
                yield return null;
            }

            // Idle at waypoint
            yield return new WaitForSeconds(patrolSettings.idleTimeAtWaypoint);

            // Move to next waypoint
            _currentWp = (_currentWp + 1) % waypoints.Length;
        }
    }

    private IEnumerator PatrolIdleThenWalk()
    {
        // New behavior: idle, then walk to next waypoint
        while (true)
        {
            // Start by idling
            _isPatrolIdling = true;
            yield return new WaitForSeconds(patrolSettings.idleTimeAtWaypoint);

            // Then walk to waypoint
            _isPatrolIdling = false;

            // Wait until we reach the waypoint
            while (Vector2.Distance(transform.position, waypoints[_currentWp].position) > patrolSettings.waypointReachDistance)
            {
                yield return null;
            }

            // Move to next waypoint index for next cycle
            _currentWp = (_currentWp + 1) % waypoints.Length;
        }
    }

    private IEnumerator AdvanceAfterPause()
    {
        // For WalkThenIdle pattern only
        yield return new WaitForSeconds(patrolSettings.idleTimeAtWaypoint);
        _currentWp = (_currentWp + 1) % waypoints.Length;
        _patrolRoutine = null;
    }

    private void FollowTick()
    {
        if (followTarget == null)
        {
            _desiredMove = Vector2.zero;
            return;
        }
        var to = (Vector2)(followTarget.position - transform.position);
        if (to.magnitude <= stopDistance) _desiredMove = Vector2.zero;
        else if (to.magnitude <= chaseDistance) _desiredMove = to.normalized;
        else _desiredMove = Vector2.zero; // outside chase range
    }

    private IEnumerator WanderLoop()
    {
        Vector2 currentDir = Random.insideUnitCircle.normalized;

        while (true)
        {
            if (wanderSettings.pattern == WanderSettings.WanderPattern.IdleThenWalk)
            {
                // Start with idle
                _desiredMove = Vector2.zero;
                float idleTime = Random.Range(wanderSettings.idleIntervalRange.x, wanderSettings.idleIntervalRange.y);
                yield return new WaitForSeconds(idleTime);

                // Then walk
                Vector2 targetDir = Random.insideUnitCircle.normalized;
                float walkTime = Random.Range(wanderSettings.wanderIntervalRange.x, wanderSettings.wanderIntervalRange.y);
                float t = 0f;

                while (t < walkTime)
                {
                    // Don't update movement while paused
                    if (!(_motor.IsDialogueActive || _motor.IsTeleporting || ClockTimer.IsTimeEnded || GlobalPause.IsMinigamePaused))
                    {
                        t += Time.deltaTime;
                        currentDir = Vector2.Lerp(currentDir, targetDir, Time.deltaTime * wanderSettings.wanderTurnSpeed).normalized;
                        _desiredMove = currentDir;
                    }
                    else
                    {
                        _desiredMove = Vector2.zero;
                    }
                    yield return null;
                }
            }
            else // WalkThenIdle (original behavior)
            {
                // Start with walk
                Vector2 targetDir = Random.insideUnitCircle.normalized;
                float walkTime = Random.Range(wanderSettings.wanderIntervalRange.x, wanderSettings.wanderIntervalRange.y);
                float t = 0f;

                while (t < walkTime)
                {
                    // Don't update movement while paused
                    if (!(_motor.IsDialogueActive || _motor.IsTeleporting || ClockTimer.IsTimeEnded || GlobalPause.IsMinigamePaused))
                    {
                        t += Time.deltaTime;
                        currentDir = Vector2.Lerp(currentDir, targetDir, Time.deltaTime * wanderSettings.wanderTurnSpeed).normalized;
                        _desiredMove = currentDir;
                    }
                    else
                    {
                        _desiredMove = Vector2.zero;
                    }
                    yield return null;
                }

                // Then idle
                _desiredMove = Vector2.zero;
                float idleTime = Random.Range(wanderSettings.idleIntervalRange.x, wanderSettings.idleIntervalRange.y);
                yield return new WaitForSeconds(idleTime);
            }
        }
    }

    // Public method to switch modes at runtime
    public void SetMode(NpcMode newMode)
    {
        if (mode == newMode) return;

        // Clean up old mode
        if (mode == NpcMode.Wander && _wanderRoutine != null)
        {
            StopCoroutine(_wanderRoutine);
            _wanderRoutine = null;
        }

        if (mode == NpcMode.Patrol && _patrolRoutine != null)
        {
            StopCoroutine(_patrolRoutine);
            _patrolRoutine = null;
        }

        mode = newMode;

        // Initialize new mode
        if (mode == NpcMode.Wander && isActiveAndEnabled)
        {
            _wanderRoutine = StartCoroutine(WanderLoop());
        }
        else if (mode == NpcMode.Patrol && isActiveAndEnabled)
        {
            StartPatrol();
        }
    }
}