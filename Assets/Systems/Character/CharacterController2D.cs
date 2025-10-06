using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class CharacterController2D : MonoBehaviour
{
    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical = Animator.StringToHash("vertical");
    private Rigidbody2D _rigidbody2d;
    
    [SerializeField] float speed = 2f;

    private Vector2 _motionVector;
    private Vector2 _lastMotionVector;

    private Animator _animator;
    
    private bool _isDialogueActive = false;
    private bool _isTeleporting = false;

    private void StopMovement()
    {
        _motionVector = Vector2.zero;
        _animator.speed = 0f;
        _rigidbody2d.linearVelocity = Vector2.zero;
    }
    
    public void SetDialogueActive(bool active) {
        _isDialogueActive = active;
        
        if (active)
        {
            StopMovement();
        } else {
            _animator.speed = 1f;
        }
    }

    public bool IsDialogueActive => _isDialogueActive;
    
    public void SetTeleporting(bool teleporting) {
        _isTeleporting = teleporting;
        
        if (teleporting) {
            StopMovement();
        } else {
            _animator.speed = 1f;
        }
    }
    
    public bool IsTeleporting => _isTeleporting;

    // Awake is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _rigidbody2d.freezeRotation = true;
    }

    private void Update()
    {
        if (_isDialogueActive || _isTeleporting) return;
        
        _motionVector = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (_motionVector is { x: 0, y: 0 }) {
            // Debug.Log(lastMotionVector);
            _animator.speed = 0f;
            _animator.SetFloat(Horizontal, _lastMotionVector.x);
            _animator.SetFloat(Vertical, _lastMotionVector.y);
        } else {
            _animator.SetFloat(Horizontal, _motionVector.x);
            _animator.SetFloat(Vertical, _motionVector.y);
            _animator.speed = 1f;

            _lastMotionVector = _motionVector;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Use velocity to allow Unity physics to handle collisions
        _rigidbody2d.linearVelocity = _motionVector.normalized * speed;
    }
}
