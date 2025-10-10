using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor2D))]
public class NpcBrain2D : MonoBehaviour
{
    public enum NpcMode { Idle, Patrol, Wander, Follow, Manual }

    [Header("Mode")]
    public NpcMode mode = NpcMode.Patrol;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float waypointReachDistance = 0.1f;
    public float pauseAtWaypoint = 0.5f;

    [Header("Wander")]
    public Vector2 wanderIntervalRange = new Vector2(1.2f, 2.0f);
    public float wanderTurnSpeed = 4f; // how fast to shift direction

    [Header("Follow")]
    public Transform followTarget;
    public float chaseDistance = 4f;
    public float stopDistance  = 0.75f;

    private CharacterMotor2D _motor;
    private int _currentWp;
    private Vector2 _desiredMove;
    private Coroutine _wanderRoutine;

    void Awake() => _motor = GetComponent<CharacterMotor2D>();

    void OnEnable()
    {
        if (mode == NpcMode.Wander)
            _wanderRoutine = StartCoroutine(WanderLoop());
    }

    void OnDisable()
    {
        if (_wanderRoutine != null) StopCoroutine(_wanderRoutine);
    }

    void Update()
    {
        if (_motor.IsDialogueActive || _motor.IsTeleporting)
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

        _motor.SetMoveInput(_desiredMove);
    }
    
    public IEnumerator MoveToPosition(Vector2 target)
    {
        if (_motor.IsDialogueActive || _motor.IsTeleporting)
            yield break;
        
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            var to = (target - (Vector2)transform.position).normalized;
            _desiredMove = to;
            yield return null;
        }
        
        _desiredMove = Vector2.zero;
    }

    private void PatrolTick()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            _desiredMove = Vector2.zero;
            return;
        }

        var target = waypoints[_currentWp].position;
        var to     = (Vector2)(target - transform.position);
        if (to.magnitude <= waypointReachDistance)
        {
            _desiredMove = Vector2.zero;
            // pause briefly, then advance
            StartCoroutine(AdvanceAfterPause());
        }
        else
        {
            _desiredMove = to.normalized;
        }
    }

    private IEnumerator AdvanceAfterPause()
    {
        // prevent multiple concurrent calls
        var savedIndex = _currentWp;
        yield return new WaitForSeconds(pauseAtWaypoint);
        if (savedIndex == _currentWp)
            _currentWp = (_currentWp + 1) % waypoints.Length;
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
            // choose a new desired direction every interval
            Vector2 targetDir = Random.insideUnitCircle.normalized;
            float   t = 0f;
            float   interval = Random.Range(wanderIntervalRange.x, wanderIntervalRange.y);

            while (t < interval)
            {
                t += Time.deltaTime;
                currentDir = Vector2.Lerp(currentDir, targetDir, Time.deltaTime * wanderTurnSpeed).normalized;
                _desiredMove = currentDir;
                yield return null;
            }
        }
    }
}