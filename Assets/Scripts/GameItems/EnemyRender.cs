using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class EnemyRender : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Image component used to show the enemy artwork as a fallback or overlay.")]
    public Image artworkImage;

    [Header("Runtime")]
    public EnemyData data;

    [Header("Animator States (when using Animator Controller)")]
    [Tooltip("Idle state name in the Animator Controller")] public string idleState = "Idle";
    [Tooltip("Attack state name in the Animator Controller")] public string attackState = "Attack";
    [Tooltip("Hurt state name in the Animator Controller")] public string hurtState = "Hurt";
    [Tooltip("Death state name in the Animator Controller")] public string deathState = "Death";
    [Tooltip("Animator layer index to play states on")] public int animatorLayer = 0;
    [Tooltip("Crossfade duration when switching states")] public float crossFadeDuration = 0.08f;

    private Animator _animator;
    private PlayableGraph _graph;
    private AnimationPlayableOutput _output;
    private AnimationClipPlayable _currentPlayable;
    private bool _graphCreated;
    private Coroutine _returnToIdleRoutine;

    // Adapter to let Animator controllers authored for SpriteRenderer drive a UI Image
    private SpriteRenderer _spriteAdapter;
    private Vector3 _imageBaseScale = Vector3.one;
    private bool _capturedBaseScale;

    private bool UsingAnimatorController => data != null && data.animatorController != null;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (artworkImage == null)
        {
            // Try to find an Image on this GameObject or its children (common for UI setups)
            artworkImage = GetComponentInChildren<Image>();
        }

        // Capture the base scale so we can apply flip without stomping magnitude
        if (TryGetComponent<RectTransform>(out var rt))
        {
            _imageBaseScale = rt.localScale;
            _capturedBaseScale = true;
        }

        EnsureSpriteAdapter();
    }

    private void OnDisable()
    {
        StopGraph();
    }

    private void OnDestroy()
    {
        StopGraph();
    }

    private void LateUpdate()
    {
        // If using an AnimatorController that targets a SpriteRenderer, mirror the animated values to the UI Image
        if (!UsingAnimatorController) return;
        if (_animator == null) return;

        if (_spriteAdapter == null)
            EnsureSpriteAdapter();

        if (artworkImage == null)
        {
            artworkImage = GetComponentInChildren<UnityEngine.UI.Image>();
            if (artworkImage == null) return;
        }

        // Mirror sprite
        var spr = _spriteAdapter != null ? _spriteAdapter.sprite : null;
        if (spr != null && artworkImage.sprite != spr)
            artworkImage.sprite = spr;

        // Ensure enabled if we have a sprite
        if (spr != null && !artworkImage.enabled)
            artworkImage.enabled = true;

        // Mirror color
        if (_spriteAdapter != null)
        {
            var c = _spriteAdapter.color;
            if (artworkImage.color != c)
                artworkImage.color = c;
        }

        // Mirror flip via RectTransform scale
        if (TryGetComponent<RectTransform>(out var rt))
        {
            if (!_capturedBaseScale)
            {
                _imageBaseScale = rt.localScale;
                _capturedBaseScale = true;
            }
            float fx = (_spriteAdapter != null && _spriteAdapter.flipX) ? -1f : 1f;
            float fy = (_spriteAdapter != null && _spriteAdapter.flipY) ? -1f : 1f;
            var targetScale = new Vector3(_imageBaseScale.x * fx, _imageBaseScale.y * fy, _imageBaseScale.z);
            if (rt.localScale != targetScale)
                rt.localScale = targetScale;
        }
    }

    private void EnsureSpriteAdapter()
    {
        if (_spriteAdapter != null) return;
        _spriteAdapter = GetComponent<SpriteRenderer>();
        if (_spriteAdapter == null)
            _spriteAdapter = gameObject.AddComponent<SpriteRenderer>();

        // Keep it from rendering in world space; it's only used as an animation data source
        _spriteAdapter.enabled = false;
        _spriteAdapter.hideFlags = HideFlags.HideInInspector;
    }

    public void Bind(EnemyData enemyData)
    {
        data = enemyData;

        // Assign the provided Animator Controller if any (drag & drop from EnemyDataSO)
        if (_animator != null)
        {
            _animator.runtimeAnimatorController = data != null ? data.animatorController : null;
        }

        // Update artwork sprite if available
        if (artworkImage)
        {
            artworkImage.sprite = data != null ? data.artwork : null;
            artworkImage.enabled = artworkImage.sprite != null;
        }

        // Start idle animation if available
        PlayIdle();
    }

    public void PlayIdle()
    {
        if (UsingAnimatorController && HasState(idleState))
        {
            StopGraph();
            CrossFadeState(idleState, 0f);
            return;
        }

        // Fallback to clip-based idle
        if (data == null || data.idleClip == null)
        {
            // No idle clip: just show artwork as static
            StopGraph();
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
            // Return to idle after state ends is handled by Animator (via transitions). No coroutine here.
            return;
        }

        if (data == null || data.attackClip == null)
        {
            // Fallback: no attack clip, just briefly flash and return to idle
            if (isActiveAndEnabled)
                StartCoroutine(FallbackPulseThenIdle(0.25f));
            return;
        }
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
        {
            // Simple blink as a minimal feedback
            if (isActiveAndEnabled)
                StartCoroutine(FallbackPulseThenIdle(0.15f));
            return;
        }
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
            // No death clip; just hide artwork
            StopGraph();
            if (artworkImage != null) artworkImage.enabled = false;
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
        if (!Application.isPlaying || clip == null)
            return;

        EnsureGraph();

        // Cancel any pending return-to-idle
        if (_returnToIdleRoutine != null)
        {
            StopCoroutine(_returnToIdleRoutine);
            _returnToIdleRoutine = null;
        }

        // Clean up previous playable
        if (_currentPlayable.IsValid())
        {
            _currentPlayable.Destroy();
        }

        _currentPlayable = AnimationClipPlayable.Create(_graph, clip);

        // Attempt to set looping by clip settings or via duration trick
        // If the clip is marked as looping in import settings, it will loop.
        // Otherwise, for UI purposes we simulate looping by restarting on complete via coroutine.
        _output.SetSourcePlayable(_currentPlayable);
        _graph.Play();

        // Show artwork image (some clips may animate Image sprite directly; we leave it enabled)
        if (artworkImage != null && data != null && data.artwork != null)
            artworkImage.enabled = true;

        if (!loop && returnToIdleOnEnd && clip.length > 0f && isActiveAndEnabled)
        {
            _returnToIdleRoutine = StartCoroutine(ReturnToIdleAfter(clip.length));
        }
    }

    private IEnumerator ReturnToIdleAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _returnToIdleRoutine = null;
        // If object still active and data unchanged, go back to idle
        if (this != null && isActiveAndEnabled)
            PlayIdle();
    }

    private IEnumerator FallbackPulseThenIdle(float seconds)
    {
        if (artworkImage)
        {
            var original = artworkImage.color;
            artworkImage.color = new Color(original.r, original.g, original.b, 0.7f);
            yield return new WaitForSeconds(seconds);
            artworkImage.color = original;
        }
        PlayIdle();
    }

    private void EnsureGraph()
    {
        if (_graphCreated)
            return;

        _graph = PlayableGraph.Create($"EnemyRenderGraph_{name}");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);
        _graphCreated = true;
    }

    private void StopGraph()
    {
        if (_returnToIdleRoutine != null)
        {
            StopCoroutine(_returnToIdleRoutine);
            _returnToIdleRoutine = null;
        }

        if (_graphCreated)
        {
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
            }
            _graph.Destroy();
            _graphCreated = false;
        }
    }
}
