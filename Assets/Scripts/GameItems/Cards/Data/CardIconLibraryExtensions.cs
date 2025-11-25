using UnityEngine;

namespace GameItems.Cards
{
    public static class CardIconLibraryExtensions
    {
        public static Sprite GetIconForIntent(this CardIconLibrary library, EnemyIntent intent)
        {
            if (library == null) return null;

            return intent switch
            {
                EnemyIntent.Attack => library.attackIcon,
                EnemyIntent.Block => library.blockIcon,
                EnemyIntent.Heal => library.healIcon,
                EnemyIntent.Buff => library.buffIcon,
                _ => library.idleIcon // Fallback to idle or a default icon
            };
        }
    }
}

