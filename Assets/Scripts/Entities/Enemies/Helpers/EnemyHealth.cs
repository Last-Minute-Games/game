using TMPro;
using UnityEngine;

namespace Entities.Enemies.Helpers
{
    public class EnemyHealth : MonoBehaviour
    {
        private TMP_Text _healthText;
        private TMP_Text _shieldText;
        private SpriteRenderer _healthBarFill;

        private void TryInitialize()
        {
            _healthText = transform.Find("HealthText")?.GetComponent<TMP_Text>();
            _shieldText = transform.Find("ShieldText")?.GetComponent<TMP_Text>();
            _healthBarFill = transform.Find("HealthBarFill")?.GetComponent<SpriteRenderer>();
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
            if (_healthText != null)
            {
                _healthText.text = $"{health} / {maxHealth}";
            }

            if (_healthBarFill != null)
            {
                float fillRatio = maxHealth > 0 ? health / (float)maxHealth : 0f;
                _healthBarFill.size = new Vector2(fillRatio, _healthBarFill.size.y);
            }
        }
    }
}
