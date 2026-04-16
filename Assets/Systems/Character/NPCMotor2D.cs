using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class NPCMotor2D : MonoBehaviour
{
    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical = Animator.StringToHash("vertical");
    private static readonly int Speed = Animator.StringToHash("speed");

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Smoothing")]
    [SerializeField] private float acceleration = 12f;

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sprite;

    private Vector2 _moveInput;
    private Vector2 _currentVelocity;
    private Vector2 _lastDirection = Vector2.down;

    // ===== STATE FLAGS (IMPORTANT FOR NPC BRAIN) =====
    private bool _isDialogueActive;
    private bool _isTeleporting;

    public bool IsDialogueActive => _isDialogueActive;
    public bool IsTeleporting => _isTeleporting;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();

        _rb.freezeRotation = true;
    }

    void Update()
    {
        bool isMoving = _moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            _lastDirection = _moveInput.normalized;
            UpdateAnimator(GetBlendDirection(_lastDirection), 1f);
        }
        else
        {
            UpdateAnimator(GetBlendDirection(_lastDirection), 0f);
        }

        UpdateSpriteFlip(_lastDirection);
    }

    private Vector2 GetBlendDirection(Vector2 dir)
    {
        // If moving more horizontally than vertically, use pure left (-1,0)
        // and let the sprite flip handle right side
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return new Vector2(-1f, 0f); // always left, flipX handles right

        // Otherwise use pure up or down
        return new Vector2(0f, dir.y > 0 ? 1f : -1f);
    }

    private Vector2 GetDominantDirection(Vector2 dir)
    {
        // If moving more horizontally, zero out vertical (and vice versa)
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return new Vector2(dir.x, 0f).normalized; // pure left/right
        else
            return new Vector2(0f, dir.y).normalized; // pure up/down
    }

    void FixedUpdate()
    {
        if (_isDialogueActive || _isTeleporting)
        {
            _rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
            return;
        }

        Vector2 targetVelocity = _moveInput.normalized * moveSpeed;

        _currentVelocity = Vector2.Lerp(
            _currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        // Snap to zero when close enough to prevent infinite glide
        if (_currentVelocity.sqrMagnitude < 0.001f)
            _currentVelocity = Vector2.zero;

        _rb.linearVelocity = _currentVelocity;
    }

    // =========================
    // ANIMATION
    // =========================
    private void UpdateAnimator(Vector2 dir, float speed)
    {
        if (!_anim.enabled) _anim.enabled = true;

        _anim.SetFloat(Horizontal, dir.x);
        _anim.SetFloat(Vertical, dir.y);
        _anim.SetFloat(Speed, speed);
    }

    private void UpdateSpriteFlip(Vector2 dir)
    {
        // Flip to show right-facing when moving right
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            _sprite.flipX = dir.x > 0f; // right = flip, left = normal
    }

    // =========================
    // PUBLIC API (USED BY NPC BRAIN)
    // =========================
    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    public void SetDialogueActive(bool active)
    {
        _isDialogueActive = active;

        if (active)
        {
            _moveInput = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
        }
    }

    public void SetTeleporting(bool t)
    {
        _isTeleporting = t;

        if (t)
        {
            _moveInput = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
        }
    }
}