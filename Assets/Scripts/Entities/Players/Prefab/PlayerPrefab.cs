using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Entities.Players.Prefab
{
    public class PlayerPrefab : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the PlayerManager that holds the runtime PlayerData")]
        public PlayerManager playerManager;

        [Tooltip("Reference to PlayerEnergyUIHelper that deals with Energy UI changes")]
        public PlayerEnergyUIHelper energyUI;

        [Tooltip("Reference to the RoundManager for timer updates")]
        public RoundManager roundManager;

        [Header("Energy UI")]
        public TextMeshProUGUI energyText;
        public Image energyIcon;

        [Header("Animation Helpers")]
        public PlayerAnimationHelper animationHelper;

        [Header("Healthbar UI")]
        public Image healthbarFill;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI shieldText;
        public Image healthbarUI;

        [Header("Low Health Visual")]
        [Tooltip("Gradient screen image that becomes visible at low health")]
        public Image gradientScreen;
        
        [Tooltip("HP percentage threshold to start showing gradient (0.3 = 30%)")]
        [Range(0f, 1f)]
        public float lowHealthThreshold = 0.3f;
        
        [Tooltip("Maximum opacity of gradient at 0 HP (0-1)")]
        [Range(0f, 1f)]
        public float maxGradientOpacity = 0.7f;

        [Header("Timer UI (from TimerPanel)")]
        [Tooltip("TimerText component from TimerPanel")]
        public TextMeshProUGUI timerText;
        
        [Tooltip("TimerFill component from TimerPanel")]
        public Image timerFill;
        
        [Tooltip("Optional TimerBG component")]
        public Image timerBG;

        [Header("Timer Visual Settings")]
        [Tooltip("Color when time is plentiful")]
        public Color normalTimerColor = Color.white;

        [Tooltip("Color when time is running low")]
        public Color warningTimerColor = Color.yellow;

        [Tooltip("Color when time is almost out")]
        public Color criticalTimerColor = Color.red;

        [Tooltip("Time threshold for warning color (seconds)")]
        public float warningThreshold = 7f;

        [Tooltip("Time threshold for critical color (seconds)")]
        public float criticalThreshold = 3f;

        [Header("Tween Settings")]
        [Tooltip("Base duration for health bar animation")]
        [SerializeField] private float baseDuration = 0.3f;
        
        [Tooltip("Duration multiplier per health point lost")]
        [SerializeField] private float durationPerHealthPoint = 0.02f;
        
        [Tooltip("Maximum tween duration")]
        [SerializeField] private float maxDuration = 1.0f;

        [Tooltip("Easing curve for health bar animation")]
        [SerializeField] private Ease tweenEase = Ease.OutCubic;

        private int _lastHealth;
        private int _lastMaxHealth;

        private int _lastBlock;
        private Sprite _cachedNormalSprite;

        private void Start()
        {
            // Try to find PlayerManager if not assigned
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }

            if (playerManager == null)
            {
                Debug.LogError("PlayerManager not found! PlayerPrefab cannot initialize.");
                return;
            }

            // Try to find RoundManager if not assigned
            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }

            if (roundManager == null)
            {
                Debug.LogWarning("RoundManager not found! Timer UI will not update.");
            }

            // Wait one frame for PlayerManager to initialize its runtime data
            StartCoroutine(InitializeAfterFrame());
        }

        private System.Collections.IEnumerator InitializeAfterFrame()
        {
            yield return null; // Wait one frame

            if (playerManager.playerData == null)
            {
                Debug.LogError("PlayerManager.playerData is null! Cannot initialize PlayerPrefab.");
                yield break;
            }

            // Initialize visuals with runtime data
            _lastHealth = playerManager.playerData.currentHealth;
            _lastMaxHealth = playerManager.playerData.maxHealth;
            SetupUI();

           // Register the UI image for animation
            if (animationHelper != null && healthbarUI != null)
                animationHelper.SetShieldedBarImage(healthbarUI);

            // Cache normal sprite AFTER UI is set
            _cachedNormalSprite = healthbarUI.sprite;

            // Cache initial block
            _lastBlock = playerManager.playerData.block;

            playerManager.playerData.OnEnergyChanged += HandleEnergyChanged;

            playerManager.playerData.worldPosition = healthbarUI.transform.position;
            playerManager.playerData.isPlayer = true;
        }

        private void HandleEnergyChanged()
        {
            // Update text ONLY
            energyText.text = $"{playerManager.playerData.currentEnergy}/{playerManager.playerData.maxEnergy}";

            // If you want gray-out at 0, add this line:
            energyIcon.color = (playerManager.playerData.currentEnergy > 0)
                ? Color.white
                : new Color(0.5f, 0.5f, 0.5f);

            // If you want pop animation:
            energyText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }

        private void Update()
        {
            // Continuously update UI to reflect runtime state
            if (playerManager != null && playerManager.playerData != null)
            {
                UpdateUI();
            }

            // Update timer UI
            UpdateTimerUI();

            playerManager.playerData.worldPosition = healthbarUI.transform.position;
        }

        private void SetupUI()
        {
            var data = playerManager.playerData;

            // Set Energy
            if (energyText != null)
                energyText.text = $"{data.currentEnergy}/{data.maxEnergy}";

            // Set Healthbar
            if (healthbarFill != null)
            {
                healthbarFill.fillAmount = data.maxHealth > 0 ? data.currentHealth / (float)data.maxHealth : 0f;
                healthbarFill.color = new Color32(108, 15, 15, 255);
            }

            if (healthText != null)
                healthText.text = $"{data.currentHealth}/{data.maxHealth}";
            
            if (shieldText != null)
                shieldText.text = data.block > 0 ? data.block.ToString() : "";
        }

        private void UpdateUI()
        {
            var data = playerManager.playerData;

            // Update Energy
            if (energyText != null)
                energyText.text = $"{data.currentEnergy}/{data.maxEnergy}";

            // Update Health with tween animation
            if (healthText != null)
                healthText.text = $"{data.currentHealth}/{data.maxHealth}";

            if (healthbarFill != null)
            {
                float targetFillAmount = data.maxHealth > 0 ? data.currentHealth / (float)data.maxHealth : 0f;
                
                // Only animate if health changed
                if (data.currentHealth != _lastHealth || data.maxHealth != _lastMaxHealth)
                {
                    int healthChange = Mathf.Abs(data.currentHealth - _lastHealth);
                    
                    // Calculate dynamic duration based on health change
                    float duration = baseDuration + (healthChange * durationPerHealthPoint);
                    duration = Mathf.Clamp(duration, baseDuration, maxDuration);

                    // Kill any existing tween and animate to new value
                    healthbarFill.DOKill();
                    healthbarFill.DOFillAmount(targetFillAmount, duration)
                        .SetEase(tweenEase);

                    _lastHealth = data.currentHealth;
                    _lastMaxHealth = data.maxHealth;
                }
            }

            int block = data.block;

            // Update Shield
            if (shieldText != null)
            {
                shieldText.text = block > 0 ? block.ToString() : "";
                shieldText.color = Color.cyan; // blue color for shield
            }

            // --- Block UI turned ON (0 → >0) ---
            if (_lastBlock <= 0 && block > 0)
            {
                // start shield animation
                if (animationHelper != null)
                    animationHelper.PlayShieldedBarAnimation(true);
            }

            // --- Block UI turned OFF (>0 → 0) ---
            if (_lastBlock > 0 && block <= 0)
            {
                // stop animation
                if (animationHelper != null)
                    animationHelper.PlayShieldedBarAnimation(false);

                // restore original sprite
                if (healthbarUI != null && _cachedNormalSprite != null)
                    healthbarUI.sprite = _cachedNormalSprite;
            }

            _lastBlock = block;

            // Update gradient screen based on health
            UpdateGradientScreen(data);
        }

        /// <summary>
        /// Updates the gradient screen opacity based on current health percentage
        /// </summary>
        private void UpdateGradientScreen(Data.PlayerData data)
        {
            if (gradientScreen == null || data.maxHealth <= 0)
                return;

            // Calculate health percentage
            float healthPercent = data.currentHealth / (float)data.maxHealth;

            // Calculate opacity based on health
            float opacity = 0f;
            
            if (healthPercent <= lowHealthThreshold)
            {
                // Map health percentage (0 to lowHealthThreshold) to opacity (maxOpacity to 0)
                // When HP is 0%, opacity is maxGradientOpacity
                // When HP is at threshold, opacity is 0
                float normalizedHealth = healthPercent / lowHealthThreshold; // 0 to 1
                opacity = maxGradientOpacity * (1f - normalizedHealth); // Inverted so low health = high opacity
            }

            // Apply opacity to gradient screen
            Color currentColor = gradientScreen.color;
            currentColor.a = opacity;
            gradientScreen.color = currentColor;
        }

        /// <summary>
        /// Updates the timer UI based on RoundManager state
        /// </summary>
        private void UpdateTimerUI()
        {
            if (roundManager == null || !roundManager.playerTurn || !roundManager.battleActive)
            {
                // Hide timer when not player's turn
                if (timerText != null) timerText.text = "";
                if (timerFill != null) timerFill.fillAmount = 0f;
                return;
            }

            float timeRemaining = roundManager.currentTurnTime;
            float timeLimit = roundManager.turnTimeLimit;

            // Update text
            if (timerText != null)
            {
                timerText.text = $"{Mathf.CeilToInt(timeRemaining)}s";

                // Update color based on time remaining
                if (timeRemaining <= criticalThreshold)
                {
                    timerText.color = criticalTimerColor;
                }
                else if (timeRemaining <= warningThreshold)
                {
                    timerText.color = warningTimerColor;
                }
                else
                {
                    timerText.color = normalTimerColor;
                }
            }

            // Update fill image
            if (timerFill != null && timeLimit > 0)
            {
                timerFill.fillAmount = timeRemaining / timeLimit;

                // Update fill color
                if (timeRemaining <= criticalThreshold)
                {
                    timerFill.color = criticalTimerColor;
                }
                else if (timeRemaining <= warningThreshold)
                {
                    timerFill.color = warningTimerColor;
                }
                else
                {
                    timerFill.color = normalTimerColor;
                }
            }
        }

        /// <summary>
        /// Force an immediate UI update (useful for events)
        /// </summary>
        public void RefreshUI()
        {
            if (playerManager != null && playerManager.playerData != null)
            {
                UpdateUI();
            }
        }
    }
}

