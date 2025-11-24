using UnityEngine;

namespace GameItems.Cards
{
    [CreateAssetMenu(menuName = "Cards/Icon Library", fileName = "CardIconLibrary")]
    public class CardIconLibrary : ScriptableObject
    {
        public Sprite attackIcon;
        public Sprite blockIcon;
        public Sprite healIcon;
        public Sprite drawIcon;
        public Sprite buffIcon;
        public Sprite debuffIcon;
        public Sprite curseIcon;
        public Sprite idleIcon;

        public Sprite GetIcon(CardCategory category)
        {
            return category switch
            {
                CardCategory.Attack => attackIcon,
                CardCategory.Block => blockIcon,
                CardCategory.Heal => healIcon,
                CardCategory.Draw => drawIcon,
                CardCategory.Buff => buffIcon,
                CardCategory.Debuff => debuffIcon,
                CardCategory.Curse => curseIcon,
                CardCategory.Idle => idleIcon,
                _ => null
            };
        }
    }
}

