using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class CharacterMotor2D : MonoBehaviour
{
    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical   = Animator.StringToHash("vertical");

    [Header("Movement")]
    [SerializeField] private float speed = 2f;

    [Header("Idle Sprites (static frames)")]
    [SerializeField] public Sprite idleUp;
    [SerializeField] public Sprite idleDown;
    [SerializeField] public Sprite idleLeft;
    [SerializeField] public Sprite idleRight;

    [Header("Direction Tuning")]
    [Tooltip("Vertical must exceed horizontal by at least this amount to count as Up/Down.")]
    [SerializeField] private float axisBias = 0.05f;

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sprite;

    private Vector2 _moveInput;
    private Vector2 _lastMotion;
    private bool _isDialogueActive;
    private bool _isTeleporting;

    public enum Facing { Down, Left, Right, Up }
    private Facing _facing = Facing.Down;

    void Awake()
    {
        _rb     = GetComponent<Rigidbody2D>();
        _anim   = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb.freezeRotation = true;
    }

    void Update()
    {
        if (_isDialogueActive || _isTeleporting)
            return;

        bool isMoving = _moveInput.sqrMagnitude > 0.0001f;

        if (isMoving)
        {
            // Ensure Animator is enabled while moving
            if (!_anim.enabled) _anim.enabled = true;

            _anim.speed = 1f;
            _anim.SetFloat(Horizontal, _moveInput.x);
            _anim.SetFloat(Vertical,   _moveInput.y);

            _lastMotion = _moveInput;
            UpdateFacingFrom(_lastMotion);
        }
        else
        {
            // Static idle: disable Animator and set a single sprite
            ApplyStaticIdle();
        }
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = _moveInput.normalized * speed; // use .velocity if your Unity doesn't have linearVelocity
    }

    private void StopMovement()
    {
        _moveInput = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
        ApplyStaticIdle();
    }

    private void UpdateFacingFrom(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return;

        if (Mathf.Abs(v.y) > Mathf.Abs(v.x) + axisBias)
        {
            _facing = (v.y >= 0f) ? Facing.Up : Facing.Down;
        }
        else
        {
            _facing = (v.x >= 0f) ? Facing.Right : Facing.Left;
        }
    }
    
    public Sprite forceIdleSprite;

    private void ApplyStaticIdle()
    {
        // Pick sprite by facing
        Sprite target =
            _facing == Facing.Up    ? idleUp :
            _facing == Facing.Down  ? idleDown :
            _facing == Facing.Left  ? idleLeft :
                                      idleRight;
        
        if (forceIdleSprite)
            target = forceIdleSprite;

        // Disable animator so it doesn't overwrite SpriteRenderer's sprite this frame
        if (_anim.enabled) _anim.enabled = false;

        _sprite.sprite = target;
    }

    // ===== Public API =====
    public void SetMoveInput(Vector2 input) => _moveInput = input;

    public void SetDialogueActive(bool active)
    {
        _isDialogueActive = active;
        if (active) StopMovement();
        else if (!_anim.enabled) _anim.enabled = true; // ready to animate again
    }
    public bool IsDialogueActive => _isDialogueActive;

    public void SetTeleporting(bool t)
    {
        _isTeleporting = t;
        if (t) StopMovement();
        else if (!_anim.enabled) _anim.enabled = true;
    }
    public bool IsTeleporting => _isTeleporting;

    public void SetSpeed(float newSpeed) => speed = newSpeed;
}
