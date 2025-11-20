using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

namespace Entities.Enemies.Helpers
{
    public class EnemyHealth : MonoBehaviour
    {
        private TMP_Text _healthText;
        private TMP_Text _shieldText;
        private SpriteRenderer _healthBarFill;

        private int _lastHealth;
        private int _lastMaxHealth;

        [Header("Tween Settings")]
        [Tooltip("Base duration for health bar animation (modified by health change)")]
        [SerializeField] private float baseDuration = 0.3f;
        
        [Tooltip("Duration multiplier per health point lost (e.g., 0.02 = 20ms per HP)")]
        [SerializeField] private float durationPerHealthPoint = 0.02f;
        
        [Tooltip("Maximum tween duration to prevent overly long animations")]
        [SerializeField] private float maxDuration = 1.0f;

        [Tooltip("Easing curve for health bar animation")]
        [SerializeField] private Ease tweenEase = Ease.OutCubic;

        [Header("Shielded Health Bar Animation")]
        [Tooltip("Frames for animated shield healthbar.")]
        public Sprite[] shieldedHealthBarFrames;

        [Tooltip("FPS for shielded animation.")]
        public float shieldAnimationFPS = 12f;

        private SpriteRenderer _healthUI;
        private Sprite _originalFrameSprite;
        private Coroutine _shieldAnimRoutine;

        private void TryInitialize()
        {
            if (_healthText == null)
            {
                _healthText = transform.Find("HealthText")?.GetComponent<TMP_Text>();
                if (_healthText == null)
                {
                    Debug.LogWarning($"[EnemyHealth] HealthText not found on {gameObject.name}. Looking for child named 'HealthText'");
                }
            }
            
            if (_shieldText == null)
            {
                _shieldText = transform.Find("ShieldText")?.GetComponent<TMP_Text>();
                if (_shieldText == null)
                {
                    Debug.LogWarning($"[EnemyHealth] ShieldText not found on {gameObject.name}. Looking for child named 'ShieldText'");
                    
                    // List all children to help debug
                    Debug.Log($"[EnemyHealth] Children of {gameObject.name}:");
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Debug.Log($"  - {transform.GetChild(i).name}");
                    }
                }
            }
            
            if (_healthBarFill == null)
            {
                _healthBarFill = transform.Find("HealthBarFill")?.GetComponent<SpriteRenderer>();
            }

            // frame renderer (the thing that switches animation frames)
            if (_healthUI == null)
                _healthUI = transform.Find("HealthUI")?.GetComponent<SpriteRenderer>();

            // Cache original sprite once
            if (_healthUI != null && _originalFrameSprite == null)
                _originalFrameSprite = _healthUI.sprite;
        }

        private void Awake()
        {
            TryInitialize();
            transform.localScale = Vector3.one * 0.07f;
        }

        public void SetLocalPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

        // updated setshield to include animation helpers for shield healthbar
        public void SetShield(int shield)
        {
            TryInitialize(); // Ensure components are found

            if (_shieldText == null)
            {
                Debug.LogWarning("[EnemyHealth] ShieldText component missing!");
                return;
            }

            // When shield > 0: show number, cyan color
            if (shield > 0)
            {
                _shieldText.text = shield.ToString();
                _shieldText.color = Color.cyan;          // cyan!
                _shieldText.enabled = true;              // ensure visible

                StartShieldedBarAnimation();
            }
            else
            {
                // Shield = 0 → hide text entirely
                _shieldText.text = "";
                _shieldText.enabled = false;

                StopShieldedBarAnimation();
            }
        }

        private void StartShieldedBarAnimation()
        {
            if (_healthUI == null) return;

            if (_shieldAnimRoutine != null)
                StopCoroutine(_shieldAnimRoutine);

            if (shieldedHealthBarFrames != null && shieldedHealthBarFrames.Length > 0)
                _shieldAnimRoutine = StartCoroutine(ShieldLoop());
        }

        private void StopShieldedBarAnimation()
        {
            if (_healthUI == null) return;

            if (_shieldAnimRoutine != null)
            {
                StopCoroutine(_shieldAnimRoutine);
                _shieldAnimRoutine = null;
            }

            // Restore the original frame sprite
            if (_originalFrameSprite != null)
                _healthUI.sprite = _originalFrameSprite;
        }

        public void SetHealth(int health, int maxHealth)
        {
            TryInitialize();

            if (_healthText != null)
            {
                _healthText.text = $"{health} / {maxHealth}";
            }

            if (_healthBarFill != null)
            {
                float targetFillRatio = maxHealth > 0 ? health / (float)maxHealth : 0f;
                Vector3 targetScale = new Vector3(9.5f * targetFillRatio,
                    _healthBarFill.transform.localScale.y, _healthBarFill.transform.localScale.z);

                // Calculate health change to determine animation duration
                int healthChange = Mathf.Abs(health - _lastHealth);
                
                // Calculate dynamic duration based on health lost
                float duration = baseDuration + (healthChange * durationPerHealthPoint);
                duration = Mathf.Clamp(duration, baseDuration, maxDuration);

                // Kill any existing tween on the health bar fill
                _healthBarFill.transform.DOKill();

                // Animate the health bar fill
                _healthBarFill.transform.DOScale(targetScale, duration)
                    .SetEase(tweenEase);

                // Store current values for next comparison
                _lastHealth = health;
                _lastMaxHealth = maxHealth;
            }
        }

        private IEnumerator ShieldLoop()
        {
            int frame = 0;
            float delay = 1f / shieldAnimationFPS;

            while (true)
            {
                if (_healthUI != null)
                    _healthUI.sprite = shieldedHealthBarFrames[frame];

                frame = (frame + 1) % shieldedHealthBarFrames.Length;

                yield return new WaitForSeconds(delay);
            }
        }
    }
}
