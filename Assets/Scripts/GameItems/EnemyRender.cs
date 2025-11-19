using Entities.Enemies.Helpers;
using UnityEngine;

namespace GameItems
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class EnemyRender : MonoBehaviour
    {
        [Header("Runtime")] 
        public EnemyData data;

        [Header("Animator States (Controller-driven)")]
        public string idleState = "Idle";
        public string attackState = "Attack";
        public string hurtState = "Hurt";
        public string deathState = "Death";
        public int animatorLayer;
        public float crossFadeDuration = 0.08f;

        private SpriteRenderer _sprite;
        private EnemyHealth _health;
        private BoxCollider2D _hitboxCollider;

        // Manual sprite animation state
        private SpriteAnimation _currentAnimation;
        private float _frameTimer;
        private int _currentFrame;
        private bool _isAnimationPlaying;

        private void Awake()
        {
            _hitboxCollider = GetComponent<BoxCollider2D>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!_isAnimationPlaying || _currentAnimation == null || _currentAnimation.frames.Count == 0)
                return;

            _frameTimer += Time.deltaTime;
            float frameDuration = 1f / _currentAnimation.frameRate;

            if (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _currentFrame++;

                if (_currentFrame >= _currentAnimation.frames.Count)
                {
                    if (_currentAnimation.loop)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _isAnimationPlaying = false;
                        PlayIdle(); // Revert to idle when non-looping animation finishes
                        return;
                    }
                }

                _sprite.sprite = _currentAnimation.frames[_currentFrame];
            }
        }



        public void Bind(EnemyData enemyData)
        {
            data = enemyData;

            // Set default sprite artwork
            if (_sprite != null)
            {
                _sprite.sprite = data != null ? data.artwork : null;
                _sprite.enabled = _sprite.sprite != null;
            }
            
            _health = GetComponentInChildren<EnemyHealth>();
            
            _hitboxCollider.offset = new Vector2(0f, 0.05f);
            _hitboxCollider.size = new Vector2(0.3f, 0.3f);

            // Update health display
            UpdateHealth();

            PlayIdle();
        }

        public void UpdateHealth()
        {
            if (_health != null && data != null)
            {
                _health.SetHealth(data.currentHealth, data.maxHealth);
                _health.SetShield(data.block);
            }
        }

        public void PlayIdle()
        {
            if (data != null && data.idleAnim != null)
                PlayAnimation(data.idleAnim);
        }

        public void PlayAttack()
        {
            if (data != null && data.attackAnim != null)
                PlayAnimation(data.attackAnim);
        }

        public void PlayHurt()
        {
            if (data != null && data.hurtAnim != null)
                PlayAnimation(data.hurtAnim);
        }

        public void PlayDeath()
        {
            if (data != null && data.deathAnim != null)
            {
                PlayAnimation(data.deathAnim);
            }
            else
            {
                // Hide on death if no clip
                if (_sprite != null) _sprite.enabled = false; 
            }
        }

        private void PlayAnimation(SpriteAnimation anim)
        {
            if (anim == null || anim.frames.Count == 0)
            {
                _isAnimationPlaying = false;
                return;
            }

            _currentAnimation = anim;
            _isAnimationPlaying = true;
            _currentFrame = 0;
            _frameTimer = 0f;

            // Set the first frame immediately
            _sprite.sprite = _currentAnimation.frames[0];
        }

        // Remove all the old Playables and Animator-related methods
        // ... (CrossFadeState, HasState, PlayClip, ReturnToIdleAfterClip, EnsureGraph, StopGraph)
    }
}
