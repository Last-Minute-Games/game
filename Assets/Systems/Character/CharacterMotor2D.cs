using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class CharacterMotor2D : MonoBehaviour
{
    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical   = Animator.StringToHash("vertical");

    [SerializeField] private float speed = 2f;

    private Rigidbody2D _rb;
    private Animator _anim;

    private Vector2 _moveInput;
    private Vector2 _lastMotion;
    private bool _isDialogueActive;
    private bool _isTeleporting;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _rb.freezeRotation = true;
    }

    void Update()
    {
        if (_isDialogueActive || _isTeleporting)
            return;

        // Animate
        if (_moveInput.sqrMagnitude < 0.0001f)
        {
            _anim.speed = 0f;
            _anim.SetFloat(Horizontal, _lastMotion.x);
            _anim.SetFloat(Vertical,   _lastMotion.y);
        }
        else
        {
            _anim.speed = 1f;
            _anim.SetFloat(Horizontal, _moveInput.x);
            _anim.SetFloat(Vertical,   _moveInput.y);
            _lastMotion = _moveInput;
        }
    }

    void FixedUpdate()
    {
        // Move using physics
        _rb.linearVelocity = _moveInput.normalized * speed; // use .velocity if your Unity doesn't have linearVelocity
    }

    private void StopMovement()
    {
        _moveInput = Vector2.zero;
        _anim.speed = 0f;
        _rb.linearVelocity = Vector2.zero;
    }

    // ===== Public API =====
    public void SetMoveInput(Vector2 input) => _moveInput = input;

    public void SetDialogueActive(bool active)
    {
        _isDialogueActive = active;
        if (active) StopMovement(); else _anim.speed = 1f;
    }
    public bool IsDialogueActive => _isDialogueActive;

    public void SetTeleporting(bool t)
    {
        _isTeleporting = t;
        if (t) StopMovement(); else _anim.speed = 1f;
    }
    public bool IsTeleporting => _isTeleporting;

    public void SetSpeed(float newSpeed) => speed = newSpeed;
}