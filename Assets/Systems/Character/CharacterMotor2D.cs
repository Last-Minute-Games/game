    using UnityEngine;

    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
    public class CharacterMotor2D : MonoBehaviour
    {
        private static readonly int Horizontal = Animator.StringToHash("horizontal");
        private static readonly int Vertical   = Animator.StringToHash("vertical");
        private const string IdleControllerResourcePath = "Animation/NikolausIdle";

        [Header("Movement")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private float sprintMultiplier = 2f;
        private bool _isSprinting;

        [Header("Animation Controllers")]
        [SerializeField] private RuntimeAnimatorController walkController;
        [SerializeField] private RuntimeAnimatorController runController;
        [SerializeField] private RuntimeAnimatorController idleController;
        [SerializeField] private bool useAnimatedIdleController = false;

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

            if (useAnimatedIdleController)
            {
                ResolveIdleController();
            }
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

                // Clear mirrored idle flip while movement animations are active.
                _sprite.flipX = false;

                // Swap animator controller based on sprint state
                RuntimeAnimatorController targetController = _isSprinting ? runController : walkController;
                if (targetController == null)
                {
                    targetController = walkController != null ? walkController : runController;
                }
                if (_anim.runtimeAnimatorController != targetController && targetController != null)
                {
                    _anim.runtimeAnimatorController = targetController;
                    _anim.Rebind();
                }

                _anim.speed = 1f;
                _anim.SetFloat(Horizontal, _moveInput.x);
                _anim.SetFloat(Vertical,   _moveInput.y);

                _lastMotion = _moveInput;
                UpdateFacingFrom(_lastMotion);
            }
            else
            {
                if (!TryApplyDirectionalIdleAnimation())
                {
                    // Static idle fallback: disable Animator and set a single sprite.
                    // Reset to walk controller when stopping.
                    if (_anim.runtimeAnimatorController != walkController && walkController != null)
                    {
                        _anim.runtimeAnimatorController = walkController;
                    }

                    ApplyStaticIdle();
                }
            }
        }

        void FixedUpdate()
        {
            if (_rb.bodyType != RigidbodyType2D.Dynamic)
                return; // don’t move static or kinematic bodies

            float currentSpeed = _isSprinting ? speed * sprintMultiplier : speed;
            _rb.linearVelocity = _moveInput.normalized * currentSpeed;
        }

        private void StopMovement()
        {
            _moveInput = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            ApplyStaticIdle();
        }

        private Facing ResolveFacingFrom(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return _facing;
            }

            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) + axisBias)
            {
                return direction.y >= 0f ? Facing.Up : Facing.Down;
            }

            return direction.x >= 0f ? Facing.Right : Facing.Left;
        }

        private void UpdateFacingFrom(Vector2 v)
        {
            if (v.sqrMagnitude < 0.0001f) return;
            _facing = ResolveFacingFrom(v);
        }

        private bool TryApplyDirectionalIdleAnimation()
        {
            if (!useAnimatedIdleController)
            {
                return false;
            }

            if (forceIdleSprite != null)
            {
                return false;
            }

            RuntimeAnimatorController resolvedIdleController = ResolveIdleController();
            if (resolvedIdleController == null)
            {
                return false;
            }

            if (!_anim.enabled) _anim.enabled = true;

            if (_anim.runtimeAnimatorController != resolvedIdleController)
            {
                _anim.runtimeAnimatorController = resolvedIdleController;
                _anim.Rebind();
            }

            Vector2 facingDirection = FacingToVector(_facing);
            _anim.speed = 1f;
            _anim.SetFloat(Horizontal, facingDirection.x);
            _anim.SetFloat(Vertical, facingDirection.y);
            ApplyAnimatedIdleFacingVisual();
            return true;
        }

        private void ApplyAnimatedIdleFacingVisual()
        {
            // Imported idle uses one side orientation; mirror only when facing right.
            _sprite.flipX = _facing == Facing.Right;
        }

        private RuntimeAnimatorController ResolveIdleController()
        {
            if (idleController == null)
            {
                idleController = Resources.Load<RuntimeAnimatorController>(IdleControllerResourcePath);
            }

            return idleController;
        }

        private static Vector2 FacingToVector(Facing facing)
        {
            switch (facing)
            {
                case Facing.Up:
                    return Vector2.up;
                case Facing.Left:
                    return Vector2.left;
                case Facing.Right:
                    return Vector2.right;
                default:
                    return Vector2.down;
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

            _sprite.flipX = false;

            // Disable animator so it doesn't overwrite SpriteRenderer's sprite this frame
            if (_anim.enabled) _anim.enabled = false;

            _sprite.sprite = target;
        }

        // ===== Public API =====
        public Facing GetFacingDirection() => _facing;

        public Facing GetFacingFromVector(Vector2 direction) => ResolveFacingFrom(direction);

        public void SetFacingDirection(Facing facing)
        {
            _facing = facing;

            bool isMoving = _moveInput.sqrMagnitude > 0.0001f;
            if (isMoving && !_isTeleporting && !_isDialogueActive)
            {
                return;
            }

            if (!TryApplyDirectionalIdleAnimation())
            {
                if (_anim.runtimeAnimatorController != walkController && walkController != null)
                {
                    _anim.runtimeAnimatorController = walkController;
                }

                ApplyStaticIdle();
            }
        }

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
            if (t)
            {
                StopMovement();

                // Reset to walk controller before teleport transition
                if (_anim.runtimeAnimatorController != walkController && walkController != null)
                {
                    _anim.runtimeAnimatorController = walkController;
                }
            }
            else if (!_anim.enabled)
            {
                _anim.enabled = true;
            }
        }
        public bool IsTeleporting => _isTeleporting;

        public void SetSpeed(float newSpeed) => speed = newSpeed;
        public void SetSprinting(bool sprinting) => _isSprinting = sprinting;
    }
