using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class CharacterController2D : MonoBehaviour
{
    Rigidbody2D rigidbody2d;

    
    [SerializeField] float speed = 2f;

    Vector2 motionVector;
    Vector2 lastMotionVector;
    
    Animator animator;
    
    bool _isDialogueActive = false;
    
    public void SetDialogueActive(bool active) {
        Debug.Log(active);
        
        _isDialogueActive = active;
        
        if (active) {
            motionVector = Vector2.zero;
            animator.speed = 0f;
            rigidbody2d.linearVelocity = Vector2.zero;
        } else {
            animator.speed = 1f;
        }
    }

    public bool IsDialogueActive => _isDialogueActive;

    // Awake is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rigidbody2d.freezeRotation = true;
    }

    private void Update()
    {
        if (_isDialogueActive) return;
        
        motionVector = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (motionVector.x == 0 && motionVector.y == 0) {
            // Debug.Log(lastMotionVector);
            animator.speed = 0f;
            animator.SetFloat("horizontal", lastMotionVector.x);
            animator.SetFloat("vertical", lastMotionVector.y);
        } else {
            animator.SetFloat("horizontal", motionVector.x);
            animator.SetFloat("vertical", motionVector.y);
            animator.speed = 1f;

            lastMotionVector = motionVector;
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
        rigidbody2d.linearVelocity = motionVector.normalized * speed;
    }
}
