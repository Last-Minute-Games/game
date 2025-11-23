using Entities.Enemies.Helpers;
using UnityEngine;
using TMPro;
using DG.Tweening;

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
    
    [Header("Intent Value Text")]
    [Tooltip("Offset from icon where value text appears")]
    public Vector3 intentValueOffset = new Vector3(0.5f, -0.3f, 0f);
    [Tooltip("Font size for intent value")]
    public float intentValueFontSize = 10f;
    [Tooltip("Color for intent value text")]
    public Color intentValueColor = Color.white;

    [Header("Move Name Popup")]
    [Tooltip("Offset from enemy position where move name appears")]
    public Vector3 moveNameOffset = new Vector3(0f, -0.11f, 0f);
    [Tooltip("Font size for move name")]
    public float moveNameFontSize = 2f;
    [Tooltip("Color for move name text")]
    public Color moveNameColor = Color.yellow;
    [Tooltip("Duration of move name popup animation")]
    public float moveNameDuration = 1.5f;
    [Tooltip("How far the text moves up during animation")]
    public float moveNameFloatDistance = -0.04f;

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
    private TMP_Text _intentValueText;
    private GameObject _intentValueObject;
    private EnemyHealth _health;
    private BoxCollider2D _hitboxCollider;
    
    private TMP_Text _moveNameText;
    private GameObject _moveNameObject;

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
        
        // Create intent value text as child of icon
        _intentValueObject = new GameObject("IntentValueText");
        _intentValueObject.transform.SetParent(_intentIconObject.transform);
        _intentValueObject.transform.localPosition = intentValueOffset;
        _intentValueObject.transform.localScale = Vector3.one; // Counter parent scale
        
        // Add TextMeshPro for value display
        _intentValueText = _intentValueObject.AddComponent<TextMeshPro>();
        _intentValueText.fontSize = intentValueFontSize;
        _intentValueText.color = intentValueColor;
        _intentValueText.alignment = TextAlignmentOptions.Center;
        _intentValueText.enabled = false; // Hidden by default
        
        // Set sorting for text
        var textRenderer = _intentValueText.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerID = _intentIconSprite.sortingLayerID;
            textRenderer.sortingOrder = _intentIconSprite.sortingOrder + 1;
        }
        
        // Create move name popup text as child
        _moveNameObject = new GameObject("MoveNameText");
        _moveNameObject.transform.SetParent(transform);
        _moveNameObject.transform.localPosition = moveNameOffset;
        _moveNameObject.transform.localScale = Vector3.one * 0.2f;
        
        // Add TextMeshPro for move name display
        _moveNameText = _moveNameObject.AddComponent<TextMeshPro>();
        _moveNameText.fontSize = moveNameFontSize;
        _moveNameText.color = moveNameColor;
        _moveNameText.alignment = TextAlignmentOptions.Center;
        _moveNameText.fontStyle = FontStyles.Bold;
        _moveNameText.enabled = false; // Hidden by default
        
        // Set sorting for move name text
        var moveNameRenderer = _moveNameText.GetComponent<MeshRenderer>();
        if (moveNameRenderer != null)
        {
            moveNameRenderer.sortingLayerName = intentIconSortingLayer;
            moveNameRenderer.sortingOrder = _sprite.sortingOrder + 20; // Above everything else
        }
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
        data.worldPosition = transform.position;
    }


    public void Bind(EnemyData enemyData)
    {
        data = enemyData;
        data.worldPosition = transform.position;
        data.isPlayer = false;

        // Set default sprite artwork
        if (_sprite != null)
        {
            _sprite.sprite = data != null ? data.artwork : null;
            _sprite.enabled = _sprite.sprite != null;
        }

        // Apply visual adjustments from EnemyData
        if (data != null)
        {
            // Apply position offset
            transform.localPosition += data.positionOffset;
            
            // Apply scale offset
            transform.localScale = Vector3.Scale(transform.localScale, data.scaleOffset);
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
            
            // Show intent value if > 0
            if (_intentValueText != null && data.intentValue > 0)
            {
                _intentValueText.text = data.intentValue.ToString();
                _intentValueText.enabled = true;
            }
            else if (_intentValueText != null)
            {
                _intentValueText.enabled = false;
            }
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
        
        if (_intentValueText != null)
        {
            _intentValueText.enabled = false;
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

    /// <summary>
    /// Shows a popup text with the move name that floats up and fades out.
    /// Call this immediately before the enemy executes their action.
    /// </summary>
    /// <param name="moveName">The name of the move to display</param>
    /// <param name="onComplete">Optional callback when animation completes</param>
    public void ShowMoveNamePopup(string moveName, System.Action onComplete = null)
    {
        if (_moveNameText == null || string.IsNullOrEmpty(moveName))
        {
            onComplete?.Invoke();
            return;
        }

        // Set the move name text
        _moveNameText.text = moveName.ToUpper();
        _moveNameText.enabled = true;

        // Reset position and alpha
        _moveNameObject.transform.localPosition = moveNameOffset;
        _moveNameText.alpha = 1f;

        // Kill any existing tweens on this object
        DG.Tweening.DOTween.Kill(_moveNameObject.transform);
        DG.Tweening.DOTween.Kill(_moveNameText);

        // Create animation sequence
        DG.Tweening.Sequence moveSequence = DG.Tweening.DOTween.Sequence();

        // Float up
        moveSequence.Append(_moveNameObject.transform
            .DOLocalMoveY(moveNameOffset.y + moveNameFloatDistance, moveNameDuration)
            .SetEase(DG.Tweening.Ease.OutCubic));

        // Fade out in the last half of the animation
        moveSequence.Join(_moveNameText
            .DOFade(0f, moveNameDuration * 0.5f)
            .SetDelay(moveNameDuration * 0.5f));

        // Hide and callback when complete
        moveSequence.OnComplete(() =>
        {
            if (_moveNameText != null)
                _moveNameText.enabled = false;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Gets the move name based on the current intent.
    /// </summary>
    public string GetMoveNameForIntent(EnemyIntent intent)
    {
        return intent switch
        {
            EnemyIntent.Attack => "Attack",
            EnemyIntent.Block => "Defend",
            EnemyIntent.Heal => "Heal",
            EnemyIntent.Buff => "Buff",
            _ => "???"
        };
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

        // if (data != null && data.deathAnim != null)
        // {
        //     PlayAnimation(data.deathAnim);
        // }
        // else
        // {
        //     // Hide on death if no clip
        //     if (_sprite != null) _sprite.enabled = false;
        // }
        
        // run death through coroutine
        if (data != null && data.deathAnim != null)
        {
            StartCoroutine(PlayDeathThenDestroy());
        }
        else
        {
            Destroy(gameObject);
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


    private System.Collections.IEnumerator PlayDeathThenDestroy()
    {
        // play the manual sprite animation
        PlayAnimation(data.deathAnim);

        // wait for death animation duration
        float duration = data.deathAnim.Duration;
        if (duration <= 0) duration = 0.5f;

        yield return new WaitForSeconds(duration);

        Destroy(gameObject);
    }

    // Remove all the old Playables and Animator-related methods
    // ... (CrossFadeState, HasState, PlayClip, ReturnToIdleAfterClip, EnsureGraph, StopGraph)
}
