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
        public float initialIdleTime = 1.0f;
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

    // ===== PLAYER DETECTION (TAG-BASED) =====
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detectionRadius = 1.0f;

    private Collider2D _detectedPlayer;
    private Rigidbody2D _rb;
    private bool _playerDetected;
    private NpcMode _previousMode;
    // =======================================

    private CharacterMotor2D _motor;
    private int _currentWp;
    private Vector2 _desiredMove;
    private Coroutine _wanderRoutine;
    private Coroutine _patrolRoutine;
    private bool _isPatrolIdling;

    private bool _isPaused;
    private float _chaseDistanceSqr;
    private float _stopDistanceSqr;
    private float _waypointReachDistanceSqr;

    void Awake()
    {
        _motor = GetComponent<CharacterMotor2D>();
        _rb = GetComponent<Rigidbody2D>();

        _chaseDistanceSqr = chaseDistance * chaseDistance;
        _stopDistanceSqr = stopDistance * stopDistance;
    }

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
        _isPaused = _motor.IsDialogueActive || _motor.IsTeleporting || ClockTimer.IsTimeEnded || GlobalPause.IsMinigamePaused;

        if (_isPaused)
        {
            _motor.SetMoveInput(Vector2.zero);
            return;
        }

        // ===== CHECK PLAYER BY TAG IN RADIUS =====
        CheckPlayerNearby();

        if (_playerDetected)
        {
            _desiredMove = Vector2.zero;
            _motor.SetMoveInput(Vector2.zero);
            return;
        }
        // ========================================

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
                if (followTarget != null)
                {
                    Vector2 offset = (Vector2)followTarget.position - (Vector2)transform.position;
                    if (offset.sqrMagnitude < _chaseDistanceSqr)
                    {
                        FollowTick();
                    }
                }
                break;
        }

        _motor.SetMoveInput(_desiredMove);
    }

    // ===== TAG-BASED DETECTION =====
    private void CheckPlayerNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        _detectedPlayer = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(playerTag))
            {
                _detectedPlayer = hit;
                break;
            }
        }

        if (_detectedPlayer != null)
        {
            if (!_playerDetected)
            {
                _playerDetected = true;
                _previousMode = mode;
                SetMode(NpcMode.Idle);
            }

            _desiredMove = Vector2.zero;
            _motor.SetMoveInput(Vector2.zero);

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            if (_playerDetected)
            {
                _playerDetected = false;

                if (_rb != null)
                    _rb.bodyType = RigidbodyType2D.Dynamic;

                SetMode(_previousMode);
            }
        }
    }
    // =================================

    // ===== DEBUG RADIUS =====
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    // ========================

    public IEnumerator MoveToPosition(Vector2 target)
    {
        if (_motor.IsDialogueActive || _motor.IsTeleporting)
            yield break;

        const float stopDistanceSqr = 0.01f;
        Vector2 offset;

        while (true)
        {
            offset = target - (Vector2)transform.position;
            if (offset.sqrMagnitude <= stopDistanceSqr)
                break;

            if (_motor.IsDialogueActive || _motor.IsTeleporting || ClockTimer.IsTimeEnded || GlobalPause.IsMinigamePaused)
            {
                _desiredMove = Vector2.zero;
                yield return null;
                continue;
            }

            _desiredMove = offset.normalized;
            yield return null;
        }

        _desiredMove = Vector2.zero;
    }

    private void StartPatrol()
    {
        if (_patrolRoutine != null) StopCoroutine(_patrolRoutine);

        _waypointReachDistanceSqr = patrolSettings.waypointReachDistance * patrolSettings.waypointReachDistance;

        if (patrolSettings.pattern == PatrolSettings.PatrolPattern.IdleThenWalk)
            _patrolRoutine = StartCoroutine(PatrolIdleThenWalk());
        else
            _patrolRoutine = StartCoroutine(PatrolWalkThenIdle());
    }

    private void PatrolTick()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            _desiredMove = Vector2.zero;
            return;
        }

        if (patrolSettings.pattern == PatrolSettings.PatrolPattern.IdleThenWalk)
        {
            if (_isPatrolIdling)
                _desiredMove = Vector2.zero;
            else
            {
                var target = waypoints[_currentWp].position;
                var to = (Vector2)(target - transform.position);
                _desiredMove = to.normalized;
            }
        }
        else
        {
            var target = waypoints[_currentWp].position;
            var to = (Vector2)(target - transform.position);

            if (to.sqrMagnitude <= _waypointReachDistanceSqr)
            {
                _desiredMove = Vector2.zero;
                if (_patrolRoutine == null)
                    _patrolRoutine = StartCoroutine(AdvanceAfterPause());
            }
            else
            {
                _desiredMove = to.normalized;
            }
        }
    }

    private IEnumerator PatrolWalkThenIdle()
    {
        if (waypoints == null || waypoints.Length == 0) yield break;

        while (true)
        {
            while (((Vector2)transform.position - (Vector2)waypoints[_currentWp].position).sqrMagnitude > _waypointReachDistanceSqr)
                yield return null;

            yield return new WaitForSeconds(patrolSettings.idleTimeAtWaypoint);
            _currentWp = (_currentWp + 1) % waypoints.Length;
        }
    }

    private IEnumerator PatrolIdleThenWalk()
    {
        if (waypoints == null || waypoints.Length == 0) yield break;

        while (true)
        {
            _isPatrolIdling = true;
            yield return new WaitForSeconds(patrolSettings.idleTimeAtWaypoint);

            _isPatrolIdling = false;

            while (((Vector2)transform.position - (Vector2)waypoints[_currentWp].position).sqrMagnitude > _waypointReachDistanceSqr)
                yield return null;

            _currentWp = (_currentWp + 1) % waypoints.Length;
        }
    }

    private IEnumerator AdvanceAfterPause()
    {
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
        float distSqr = to.sqrMagnitude;

        if (distSqr <= _stopDistanceSqr)
            _desiredMove = Vector2.zero;
        else if (distSqr <= _chaseDistanceSqr)
            _desiredMove = to.normalized;
        else
            _desiredMove = Vector2.zero;
    }

    private IEnumerator WanderLoop()
    {
        Vector2 currentDir = Random.insideUnitCircle.normalized;

        while (true)
        {
            if (wanderSettings.pattern == WanderSettings.WanderPattern.IdleThenWalk)
            {
                _desiredMove = Vector2.zero;
                yield return new WaitForSeconds(Random.Range(wanderSettings.idleIntervalRange.x, wanderSettings.idleIntervalRange.y));

                Vector2 targetDir = Random.insideUnitCircle.normalized;
                float walkTime = Random.Range(wanderSettings.wanderIntervalRange.x, wanderSettings.wanderIntervalRange.y);
                float t = 0f;

                while (t < walkTime)
                {
                    if (!_isPaused)
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
            else
            {
                Vector2 targetDir = Random.insideUnitCircle.normalized;
                float walkTime = Random.Range(wanderSettings.wanderIntervalRange.x, wanderSettings.wanderIntervalRange.y);
                float t = 0f;

                while (t < walkTime)
                {
                    if (!_isPaused)
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

                _desiredMove = Vector2.zero;
                yield return new WaitForSeconds(Random.Range(wanderSettings.idleIntervalRange.x, wanderSettings.idleIntervalRange.y));
            }
        }
    }

    public void SetMode(NpcMode newMode)
    {
        if (mode == newMode) return;

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

        if (mode == NpcMode.Wander && isActiveAndEnabled)
            _wanderRoutine = StartCoroutine(WanderLoop());
        else if (mode == NpcMode.Patrol && isActiveAndEnabled)
            StartPatrol();
    }
}