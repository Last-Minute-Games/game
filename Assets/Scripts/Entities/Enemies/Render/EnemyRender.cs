using Entities.Enemies.Helpers;
using UnityEngine;
using TMPro;
using DG.Tweening;
using GameItems.Cards;

namespace Entities.Enemies.Render
{
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
        public float intentIconSize = 0.2f;
        [Tooltip("Sorting layer for intent icon")]
        public string intentIconSortingLayer = "Default";
        [Tooltip("Sorting order offset from enemy sprite")]
        public int intentIconSortingOrderOffset = 10;
        
        [Header("Intent Value Text")]
        [Tooltip("Offset from icon where value text appears")]
        public Vector3 intentValueOffset = new(0.15f, -0.12f, 0f);
        [Tooltip("Font size for intent value")]
        public float intentValueFontSize = 2f;
        [Tooltip("Color for intent value text")]
        public Color intentValueColor = Color.white;

        [Header("Move Name Popup")]
        [Tooltip("Offset from enemy position where move name appears")]
        public Vector3 moveNameOffset = new(0f, -0.11f, 0f);
        [Tooltip("Font size for move name")]
        public float moveNameFontSize = 2f;
        [Tooltip("Color for move name text")]
        public Color moveNameColor = Color.yellow;
        [Tooltip("Duration of move name popup animation")]
        public float moveNameDuration = 1.5f;
        [Tooltip("How far the text moves up during animation")]
        public float moveNameFloatDistance = -0.04f;
        [Tooltip("Sound effect to play when move name appears")]
        public SFXCueData moveNameSoundCue;

        [Header("Enemy Action Sounds")]
        [Tooltip("Sound effect to play when enemy attacks")]
        public SFXCueData enemyAttackSoundCue;
        [Tooltip("Sound effect to play when enemy gains block/defends")]
        public SFXCueData enemyDefenseSoundCue;
        [Tooltip("Sound effect to play when enemy heals")]
        public SFXCueData enemyHealSoundCue;

        [Header("Hover Sprite")]
        [Tooltip("Sprite to show when enemy is hovered over")]
        public Sprite hoverSprite;
        [Tooltip("Y offset for hover sprite position")]
        public float hoverSpriteYOffset;
        [Tooltip("Sorting layer for hover sprite")]
        public string hoverSpriteSortingLayer = "Default";
        [Tooltip("Sorting order offset from enemy sprite")]
        public int hoverSpriteSortingOrderOffset = 5;
        
        [Header("Hover Sprite Animation")]
        [Tooltip("Maximum scale multiplier when hover sprite expands")]
        public float hoverSpriteMaxScale = 1.1f;
        [Tooltip("Duration of one pulse cycle (expand + contract)")]
        public float hoverSpritePulseDuration = 1f;

        [Header("Animator States (Controller-driven)")]
        public string idleState = "Idle";

        public string attackState = "Attack";
        public string hurtState = "Hurt";
        public string deathState = "Death";
        public int animatorLayer;
        public float crossFadeDuration = 0.08f;

        // References provided by EnemyPrefab hierarchy
        [Header("Prefab References")]
        [Tooltip("Root GameObject for the intent icon (child of EnemyPrefab)")]
        [SerializeField] private GameObject intentIconRoot;
        [Tooltip("SpriteRenderer used for the intent icon")] 
        [SerializeField] private SpriteRenderer intentIconSprite;
        [Tooltip("TextMeshPro component used for the intent value")] 
        [SerializeField] private TMP_Text intentValueText;
        [Tooltip("Root GameObject for the move name popup text")] 
        [SerializeField] private GameObject moveNameRoot;
        [Tooltip("TextMeshPro component used for the move name popup")] 
        [SerializeField] private TMP_Text moveNameText;
        [Tooltip("Root GameObject for the hover sprite")] 
        [SerializeField] private GameObject hoverSpriteRoot;
        [Tooltip("SpriteRenderer used for the hover overlay")] 
        [SerializeField] private SpriteRenderer hoverSpriteRenderer;

        private SpriteRenderer _sprite;
        private EnemyHealth _health;
        private BoxCollider2D _hitboxCollider;
        
        private Tween _hoverSpritePulseTween; // Tracks the pulse animation

        // Manual sprite animation state
        private SpriteAnimation _currentAnimation;
        private float _frameTimer;
        private int _currentFrame;
        private bool _isAnimationPlaying;

        private void Awake()
        {
            _hitboxCollider = GetComponent<BoxCollider2D>();
            _sprite = GetComponent<SpriteRenderer>();

            // Apply initial offsets/sizing to prefab children if they are assigned
            if (intentIconRoot != null)
            {
                intentIconRoot.transform.localPosition = intentIconOffset;
                intentIconRoot.transform.localScale = Vector3.one * intentIconSize;
            }
            if (intentIconSprite != null)
            {
                intentIconSprite.sortingLayerName = intentIconSortingLayer;
                intentIconSprite.sortingOrder = _sprite.sortingOrder + intentIconSortingOrderOffset;
                intentIconSprite.enabled = false;
            }
            if (intentValueText != null)
            {
                intentValueText.rectTransform.localPosition = intentValueOffset;
                intentValueText.fontSize = intentValueFontSize;
                intentValueText.color = intentValueColor;
                intentValueText.alignment = TextAlignmentOptions.Center;
                intentValueText.enabled = false;
            }

            if (moveNameRoot != null)
            {
                moveNameRoot.transform.localPosition = moveNameOffset;
            }
            if (moveNameText != null)
            {
                moveNameText.fontSize = moveNameFontSize;
                moveNameText.color = moveNameColor;
                moveNameText.alignment = TextAlignmentOptions.Center;
                moveNameText.fontStyle = FontStyles.Bold;
                moveNameText.enabled = false;
            }

            if (hoverSpriteRoot != null)
            {
                hoverSpriteRoot.transform.localPosition = new Vector3(0f, hoverSpriteYOffset, 0f);
            }
            if (hoverSpriteRenderer != null)
            {
                hoverSpriteRenderer.sortingLayerName = hoverSpriteSortingLayer;
                hoverSpriteRenderer.sortingOrder = _sprite.sortingOrder + hoverSpriteSortingOrderOffset;
                if (hoverSpriteRenderer.color.a == 0f)
                {
                    hoverSpriteRenderer.color = new Color32(251, 236, 93, 150);
                }
                hoverSpriteRenderer.enabled = false;
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


        public void Bind(EnemyData enemyData, Sprite hoverSpriteOverride = null, float? hoverSpriteYOffsetOverride = null, Vector3? intentIconOffsetOverride = null, float? intentIconSizeOverride = null, int? hoverSpriteSortingOrderOffsetOverride = null)
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
            
            // Apply render settings from EnemyManager (passed as parameters)
            if (hoverSpriteOverride != null)
            {
                hoverSprite = hoverSpriteOverride;
            }
            
            if (hoverSpriteYOffsetOverride.HasValue)
            {
                hoverSpriteYOffset = hoverSpriteYOffsetOverride.Value;
                
                // Update hover sprite position with new Y offset
                if (hoverSpriteRoot != null)
                {
                    hoverSpriteRoot.transform.localPosition = new Vector3(0f, hoverSpriteYOffset, 0f);
                }
            }
            
            if (intentIconOffsetOverride.HasValue)
            {
                intentIconOffset = intentIconOffsetOverride.Value;
                
                // Update intent icon object with new settings
                if (intentIconRoot != null)
                {
                    intentIconRoot.transform.localPosition = intentIconOffset;
                }
            }
            
            if (intentIconSizeOverride.HasValue)
            {
                intentIconSize = intentIconSizeOverride.Value;
                
                // Update intent icon object with new settings
                if (intentIconRoot != null)
                {
                    intentIconRoot.transform.localScale = Vector3.one * intentIconSize;
                }
            }
            
            if (hoverSpriteSortingOrderOffsetOverride.HasValue)
            {
                hoverSpriteSortingOrderOffset = hoverSpriteSortingOrderOffsetOverride.Value;
                
                // Update hover sprite sorting order
                if (hoverSpriteRenderer != null)
                {
                    hoverSpriteRenderer.sortingOrder = _sprite.sortingOrder + hoverSpriteSortingOrderOffset;
                }
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
        public void UpdateIntentIcon(GameItems.Cards.CardIconLibrary iconLibrary = null)
        {
            if (data == null || intentIconSprite == null)
            {
                HideIntentIcon();
                return;
            }

            // If no icon library provided, try to load default one
            if (iconLibrary == null)
            {
                iconLibrary = UnityEngine.Resources.Load<GameItems.Cards.CardIconLibrary>("Nether/StatusIcons/DefaultCardIconLibrary");
            }

            if (iconLibrary == null)
            {
                Debug.LogWarning($"[EnemyRender] No icon library available for {data.enemyName}");
                HideIntentIcon();
                return;
            }

            Sprite intentSprite = iconLibrary.GetIconForIntent(data.currentIntent);
            
            if (intentSprite != null)
            {
                intentIconSprite.sprite = intentSprite;
                intentIconSprite.enabled = true;
                
                // Show intent value if > 0
                if (intentValueText != null && data.intentValue > 0)
                {
                    intentValueText.text = data.intentValue.ToString();
                    intentValueText.enabled = true;
                }
                else if (intentValueText != null)
                {
                    intentValueText.enabled = false;
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
            if (intentIconSprite != null)
            {
                intentIconSprite.enabled = false;
            }
            
            if (intentValueText != null)
            {
                intentValueText.enabled = false;
            }
        }

        /// <summary>
        /// Shows the intent icon with the given sprite.
        /// </summary>
        public void ShowIntentIcon(Sprite icon)
        {
            if (intentIconSprite != null && icon != null)
            {
                intentIconSprite.sprite = icon;
                intentIconSprite.enabled = true;
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
            if (moveNameText == null || string.IsNullOrEmpty(moveName))
            {
                onComplete?.Invoke();
                return;
            }

            // Play sound effect if assigned
            if (moveNameSoundCue != null && SFXManager.Instance != null)
            {
                SFXManager.Instance.Play(moveNameSoundCue);
            }

            // Set the move name text
            moveNameText.text = moveName.ToUpper();
            moveNameText.enabled = true;

            // Reset position and alpha
            if (moveNameRoot != null)
            {
                moveNameRoot.transform.localPosition = moveNameOffset;
            }
            moveNameText.alpha = 1f;

            // Kill any existing tweens on this object
            if (moveNameRoot != null)
            {
                DOTween.Kill(moveNameRoot.transform);
            }
            DOTween.Kill(moveNameText);

            // Create animation sequence
            Sequence moveSequence = DOTween.Sequence();

            if (moveNameRoot != null)
            {
                // Float up
                moveSequence.Append(moveNameRoot.transform
                    .DOLocalMoveY(moveNameOffset.y + moveNameFloatDistance, moveNameDuration)
                    .SetEase(Ease.OutCubic));
            }

            // Fade out in the last half of the animation
            moveSequence.Join(moveNameText
                .DOFade(0f, moveNameDuration * 0.5f)
                .SetDelay(moveNameDuration * 0.5f));

            // Hide and callback when complete
            moveSequence.OnComplete(() =>
            {
                if (moveNameText != null)
                    moveNameText.enabled = false;
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Gets the move name for the given action.
        /// Returns custom name if set, otherwise returns default name based on intent.
        /// </summary>
        public string GetMoveNameForAction(EnemyAction action)
        {
            // Check if custom name is set
            if (!string.IsNullOrEmpty(action.customName))
            {
                return action.customName;
            }
            
            // Return default name based on intent
            return action.intent switch
            {
                EnemyIntent.Attack => "Attack",
                EnemyIntent.Block => "Defend",
                EnemyIntent.Heal => "Heal",
                EnemyIntent.Buff => "Buff",
                _ => "???"
            };
        }
        
        /// <summary>
        /// Gets the move name based on the current intent (legacy method).
        /// Use GetMoveNameForAction() for custom name support.
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

        /// <summary>
        /// Plays the enemy attack sound effect.
        /// Call this when the enemy is about to execute an attack action.
        /// </summary>
        public void PlayEnemyAttackSound()
        {
            Debug.Log($"[EnemyRender] PlayEnemyAttackSound called for {data?.enemyName ?? "unknown"}. AttackCue assigned: {enemyAttackSoundCue != null}, SFXManager: {SFXManager.Instance != null}");
            
            if (enemyAttackSoundCue != null && SFXManager.Instance != null)
            {
                Debug.Log($"[EnemyRender] Playing enemy attack sound: {enemyAttackSoundCue.cueName}");
                SFXManager.Instance.Play(enemyAttackSoundCue);
            }
            else if (enemyAttackSoundCue == null)
            {
                Debug.LogWarning($"[EnemyRender] Enemy attack sound cue not assigned for {data?.enemyName ?? "unknown"}");
            }
        }

        /// <summary>
        /// Plays the enemy defense sound effect.
        /// Call this when the enemy is about to execute a block/defense action.
        /// </summary>
        public void PlayEnemyDefenseSound()
        {
            Debug.Log($"[EnemyRender] PlayEnemyDefenseSound called for {data?.enemyName ?? "unknown"}. DefenseCue assigned: {enemyDefenseSoundCue != null}, SFXManager: {SFXManager.Instance != null}");
            
            if (enemyDefenseSoundCue != null && SFXManager.Instance != null)
            {
                Debug.Log($"[EnemyRender] Playing enemy defense sound: {enemyDefenseSoundCue.cueName}");
                SFXManager.Instance.Play(enemyDefenseSoundCue);
            }
            else if (enemyDefenseSoundCue == null)
            {
                Debug.LogWarning($"[EnemyRender] Enemy defense sound cue not assigned for {data?.enemyName ?? "unknown"}");
            }
        }

        /// <summary>
        /// Plays the enemy heal sound effect.
        /// Call this when the enemy is about to execute a heal action.
        /// </summary>
        public void PlayEnemyHealSound()
        {
            Debug.Log($"[EnemyRender] PlayEnemyHealSound called for {data?.enemyName ?? "unknown"}. HealCue assigned: {enemyHealSoundCue != null}, SFXManager: {SFXManager.Instance != null}");
            
            if (enemyHealSoundCue != null && SFXManager.Instance != null)
            {
                Debug.Log($"[EnemyRender] Playing enemy heal sound: {enemyHealSoundCue.cueName}");
                SFXManager.Instance.Play(enemyHealSoundCue);
            }
            else if (enemyHealSoundCue == null)
            {
                Debug.LogWarning($"[EnemyRender] Enemy heal sound cue not assigned for {data?.enemyName ?? "unknown"}");
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


        /// <summary>
        /// Shows the hover sprite overlay on the enemy.
        /// </summary>
        public void ShowHoverSprite()
        {
            if (hoverSpriteRenderer != null && hoverSprite != null)
            {
                // Only start animation if sprite wasn't already visible
                bool wasAlreadyVisible = hoverSpriteRenderer.enabled;
                
                hoverSpriteRenderer.sprite = hoverSprite;
                hoverSpriteRenderer.enabled = true;
                
                // Start pulse animation only if this is a new show (not already visible)
                if (!wasAlreadyVisible)
                {
                    StartHoverSpritePulseAnimation();
                }
            }
        }

        /// <summary>
        /// Hides the hover sprite overlay from the enemy.
        /// </summary>
        public void HideHoverSprite()
        {
            if (hoverSpriteRenderer != null)
            {
                hoverSpriteRenderer.enabled = false;
            }
            
            // Stop pulse animation
            StopHoverSpritePulseAnimation();
        }

        /// <summary>
        /// Starts the looping pulse animation for the hover sprite.
        /// </summary>
        private void StartHoverSpritePulseAnimation()
        {
            if (hoverSpriteRoot == null)
                return;
            
            // Stop any existing pulse tween
            StopHoverSpritePulseAnimation();
            
            // Create a looping sequence that expands then contracts
            Sequence pulseSequence = DOTween.Sequence();
            
            // Expand to max scale (first half of pulse)
            pulseSequence.Append(
                hoverSpriteRoot.transform
                    .DOScale(Vector3.one * hoverSpriteMaxScale, hoverSpritePulseDuration * 0.5f)
                    .SetEase(Ease.InOutSine)
            );
            
            // Contract back to normal scale (second half of pulse)
            pulseSequence.Append(
                hoverSpriteRoot.transform
                    .DOScale(Vector3.one, hoverSpritePulseDuration * 0.5f)
                    .SetEase(Ease.InOutSine)
            );
            
            // Loop forever
            pulseSequence.SetLoops(-1, LoopType.Restart);
            
            // Store reference so we can kill it later
            _hoverSpritePulseTween = pulseSequence;
        }

        /// <summary>
        /// Stops the hover sprite pulse animation.
        /// </summary>
        private void StopHoverSpritePulseAnimation()
        {
            if (_hoverSpritePulseTween != null)
            {
                _hoverSpritePulseTween.Kill();
                _hoverSpritePulseTween = null;
            }
            
            // Reset scale to normal
            if (hoverSpriteRoot != null)
            {
                hoverSpriteRoot.transform.localScale = Vector3.one;
            }
        }
    }
}
