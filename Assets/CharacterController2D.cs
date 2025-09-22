using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class CharacterController2D : MonoBehaviour
{
    Rigidbody2D rigidbody2d;
    [SerializeField] float speed = 2f;

    Vector2 motionVector;
    Vector2 lastMotionVector;
    
    Animator animator;

    // Awake is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        motionVector = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // Debug.Log(motionVector);

        if (motionVector.x == 0 && motionVector.y == 0) {
            Debug.Log(lastMotionVector);
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
        rigidbody2d.linearVelocity = motionVector * speed;
    }
}
