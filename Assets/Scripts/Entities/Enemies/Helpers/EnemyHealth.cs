using DG.Tweening;
using TMPro;
using UnityEngine;

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

        private void TryInitialize()
        {
            _healthText = transform.Find("HealthText")?.GetComponent<TMP_Text>();
            _shieldText = transform.Find("ShieldText")?.GetComponent<TMP_Text>();
            _healthBarFill = transform.Find("HealthBarFill").GetComponent<SpriteRenderer>();
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

        public void SetShield(int shield) => _shieldText.text = shield.ToString();

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
    }
}