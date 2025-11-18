using Entities.Enemies.Helpers;

namespace GameItems
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Animations;
    using UnityEngine.Playables;

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

        private Animator _animator;
        private SpriteRenderer _sprite;
        private EnemyHealth _health;
        private BoxCollider2D _hitboxCollider;

        // Playables (fallback when no AnimatorController)
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationClipPlayable _currentPlayable;
        private bool _graphCreated;
        private Coroutine _returnToIdleRoutine;

        private bool UsingAnimatorController => data != null && data.animatorController != null;

        private void Awake()
        {
            _hitboxCollider = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void OnDisable() => StopGraph();
        private void OnDestroy() => StopGraph();

        public void Bind(EnemyData enemyData)
        {
            data = enemyData;

            // Assign Animator Controller if provided
            if (_animator != null)
                _animator.runtimeAnimatorController = data != null ? data.animatorController : null;

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
                _health.SetHealth(data.entity.health, data.entity.maxHealth);
                _health.SetShield(data.entity.block);
            }
        }

        public void PlayIdle()
        {
            if (UsingAnimatorController && HasState(idleState))
            {
                StopGraph();
                CrossFadeState(idleState, 0f);
                return;
            }

            if (data == null || data.idleClip == null)
            {
                StopGraph(); // static sprite only
                return;
            }
            PlayClip(data.idleClip, loop: true, returnToIdleOnEnd: false);
        }

        public void PlayAttack()
        {
            if (UsingAnimatorController && HasState(attackState))
            {
                StopGraph();
                CrossFadeState(attackState, 0f);
                return;
            }

            if (data == null || data.attackClip == null)
                return;

            PlayClip(data.attackClip, loop: false, returnToIdleOnEnd: true);
        }

        public void PlayHurt()
        {
            if (UsingAnimatorController && HasState(hurtState))
            {
                StopGraph();
                CrossFadeState(hurtState, 0f);
                return;
            }

            if (data == null || data.hurtClip == null)
                return;

            PlayClip(data.hurtClip, loop: false, returnToIdleOnEnd: true);
        }

        public void PlayDeath()
        {
            if (UsingAnimatorController && HasState(deathState))
            {
                StopGraph();
                CrossFadeState(deathState, 0f);
                return;
            }

            if (data == null || data.deathClip == null)
            {
                StopGraph();
                if (_sprite != null) _sprite.enabled = false; // hide on death if no clip
                return;
            }
            PlayClip(data.deathClip, loop: false, returnToIdleOnEnd: false);
        }

        private void CrossFadeState(string stateName, float normalizedTime)
        {
            if (_animator == null) return;
            _animator.CrossFade(stateName, crossFadeDuration, animatorLayer, normalizedTime);
        }

        private bool HasState(string stateName)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return false;
            int hash = Animator.StringToHash(stateName);
            return _animator.HasState(animatorLayer, hash);
        }

        private void PlayClip(AnimationClip clip, bool loop, bool returnToIdleOnEnd)
        {
            if (!Application.isPlaying || clip == null) return;
            EnsureGraph();

            if (_returnToIdleRoutine != null)
            {
                StopCoroutine(_returnToIdleRoutine);
                _returnToIdleRoutine = null;
            }

            if (_currentPlayable.IsValid())
                _currentPlayable.Destroy();

            _currentPlayable = AnimationClipPlayable.Create(_graph, clip);
            _currentPlayable.SetApplyFootIK(false);
            _currentPlayable.SetApplyPlayableIK(false);
            _currentPlayable.SetTime(0);

            _output.SetSourcePlayable(_currentPlayable);
            _graph.Play();

            if (!loop && returnToIdleOnEnd)
                _returnToIdleRoutine = StartCoroutine(ReturnToIdleAfter(clip.length));
        }

        private IEnumerator ReturnToIdleAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            PlayIdle();
        }

        private void EnsureGraph()
        {
            if (_graphCreated) return;
            _graph = PlayableGraph.Create($"EnemyRenderGraph_{name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _output = AnimationPlayableOutput.Create(_graph, "EnemyAnimOutput", _animator);
            _graphCreated = true;
        }

        private void StopGraph()
        {
            if (_returnToIdleRoutine != null)
            {
                StopCoroutine(_returnToIdleRoutine);
                _returnToIdleRoutine = null;
            }
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
            }
            if (_graphCreated && _graph.IsValid())
            {
                _graph.Destroy();
            }
            _graphCreated = false;
        }
    }
}
