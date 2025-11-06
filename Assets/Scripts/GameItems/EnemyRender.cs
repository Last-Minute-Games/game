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

    private Animator _animator;
    private PlayableGraph _graph;
    private AnimationPlayableOutput _output;
    private AnimationClipPlayable _currentPlayable;
    private bool _graphCreated;
    private Coroutine _returnToIdleRoutine;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (artworkImage == null)
        {
            // Try to find an Image on this GameObject or its children (common for UI setups)
            artworkImage = GetComponentInChildren<Image>();
        }
    }

    private void OnDisable()
    {
        StopGraph();
    }

    private void OnDestroy()
    {
        StopGraph();
    }

    public void Bind(EnemyData enemyData)
    {
        data = enemyData;

        // Update artwork sprite if available
        if (artworkImage != null)
        {
            artworkImage.sprite = data != null ? data.artwork : null;
            artworkImage.enabled = artworkImage.sprite != null;
        }

        // Start idle animation if available
        PlayIdle();
    }

    public void PlayIdle()
    {
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
        if (data == null || data.attackClip == null)
        {
            // Fallback: no attack clip, just briefly flash and return to idle
            if (isActiveAndEnabled)
                StartCoroutine(FallbackPulseThenIdle(0.25f));
            return;
        }
        PlayClip(data.attackClip, loop: false, returnToIdleOnEnd: true);
    }

    public void PlayDeath()
    {
        if (data == null || data.deathClip == null)
        {
            // No death clip; just hide artwork
            StopGraph();
            if (artworkImage != null) artworkImage.enabled = false;
            return;
        }
        PlayClip(data.deathClip, loop: false, returnToIdleOnEnd: false);
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
