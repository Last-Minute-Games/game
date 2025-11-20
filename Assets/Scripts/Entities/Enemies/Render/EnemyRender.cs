using Entities.Enemies.Helpers;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyRender : MonoBehaviour
{
    [Header("Runtime")] public EnemyData data;

    [Header("Intent Icon")]
    [Tooltip("Offset from enemy position where intent icon appears")]
    public Vector3 intentIconOffset = new Vector3(0f, 0.23f, 0f);
    [Tooltip("Size of the intent icon sprite")]
    public float intentIconSize = 0.04f;
    [Tooltip("Sorting layer for intent icon")]
    public string intentIconSortingLayer = "Default";
    [Tooltip("Sorting order offset from enemy sprite")]
    public int intentIconSortingOrderOffset = 10;

    [Header("Animator States (Controller-driven)")]
    public string idleState = "Idle";

    public string attackState = "Attack";
    public string hurtState = "Hurt";
    public string deathState = "Death";
    public int animatorLayer;
    public float crossFadeDuration = 0.08f;

    private SpriteRenderer _sprite;
    private SpriteRenderer _intentIconSprite;
    private GameObject _intentIconObject;
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

        // Create intent icon GameObject as child
        _intentIconObject = new GameObject("IntentIcon");
        _intentIconObject.transform.SetParent(transform);
        _intentIconObject.transform.localPosition = intentIconOffset;
        _intentIconObject.transform.localScale = Vector3.one * intentIconSize;

        // Add SpriteRenderer for intent icon
        _intentIconSprite = _intentIconObject.AddComponent<SpriteRenderer>();
        _intentIconSprite.sortingLayerName = intentIconSortingLayer;
        _intentIconSprite.sortingOrder = _sprite.sortingOrder + intentIconSortingOrderOffset;
        _intentIconSprite.enabled = false; // Hidden by default
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

        // Update intent icon
        UpdateIntentIcon();

        PlayIdle();
    }

    /// <summary>
    /// Updates the intent icon based on the current enemy data intent.
    /// Call this after rolling intents or when intent changes.
    /// </summary>
    public void UpdateIntentIcon()
    {
        if (data == null || _intentIconSprite == null)
        {
            HideIntentIcon();
            return;
        }

        Sprite intentSprite = data.GetCurrentIntentIcon();
        
        if (intentSprite != null)
        {
            _intentIconSprite.sprite = intentSprite;
            _intentIconSprite.enabled = true;
        }
        else
        {
            HideIntentIcon();
            Debug.LogWarning($"[EnemyRender] No intent icon for {data.enemyName} with intent {data.currentIntent}");
        }
    }

    /// <summary>
    /// Hides the intent icon (e.g., when enemy is dead or has no intent).
    /// </summary>
    public void HideIntentIcon()
    {
        if (_intentIconSprite != null)
        {
            _intentIconSprite.enabled = false;
        }
    }

    /// <summary>
    /// Shows the intent icon with the given sprite.
    /// </summary>
    public void ShowIntentIcon(Sprite icon)
    {
        if (_intentIconSprite != null && icon != null)
        {
            _intentIconSprite.sprite = icon;
            _intentIconSprite.enabled = true;
        }
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
        // Hide intent icon on death
        HideIntentIcon();

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