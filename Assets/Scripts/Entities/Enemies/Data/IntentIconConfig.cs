using UnityEngine;

namespace Entities.Enemies.Data
{
    /// <summary>
    /// ScriptableObject to store global intent icon mappings.
    /// Create one via: Assets → Create → Enemy → Intent Icon Config
    /// Then assign it to your EnemyManager or individual enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "IntentIconConfig", menuName = "Enemy/Intent Icon Config", order = 1)]
    public class IntentIconConfig : ScriptableObject
    {
        [Header("Intent Icons")]
        [Tooltip("Icon shown when enemy intends to attack")]
        public Sprite attackIcon;
        
        [Tooltip("Icon shown when enemy intends to block/defend")]
        public Sprite blockIcon;
        
        [Tooltip("Icon shown when enemy intends to heal")]
        public Sprite healIcon;
        
        [Tooltip("Icon shown when enemy intends to buff")]
        public Sprite buffIcon;

        /// <summary>
        /// Get the appropriate icon sprite for the given intent.
        /// </summary>
        public Sprite GetIconForIntent(EnemyIntent intent)
        {
            return intent switch
            {
                EnemyIntent.Attack => attackIcon,
                EnemyIntent.Block => blockIcon,
                EnemyIntent.Heal => healIcon,
                EnemyIntent.Buff => buffIcon,
                _ => null
            };
        }

        /// <summary>
        /// Converts this config to an IntentIconMapping instance.
        /// Useful for assigning to EnemyData.
        /// </summary>
        public IntentIconMapping ToMapping()
        {
            return new IntentIconMapping
            {
                attackIcon = attackIcon,
                blockIcon = blockIcon,
                healIcon = healIcon,
                buffIcon = buffIcon
            };
        }
    }
}

