namespace GameItems.Cards
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Runtime instance of a CardData with its rolled effect values.
    /// Keeps the original CardData reference but stores cloned EffectData
    /// with postCopyValue set at draw time.
    /// </summary>
    [System.Serializable]
    public class CardInstance
    {
        public CardData data;
        public List<EffectData> rolledEffects = new();

        // Optional: variability tier for UI hints if applicable (only when a single effect exists)
        public CardVariationTier? tier;

        public static CardInstance FromData(CardData source, bool applyVariability)
        {
            if (source == null)
            {
                Debug.LogWarning("[CardInstance] FromData called with null CardData.");
                return null;
            }

            var inst = new CardInstance { data = source };

            // Clone each effect with multiplier if variability is on for this card
            bool apply = applyVariability && source.isVariableCard;
            if (source.effectData != null)
            {
                foreach (var eff in source.effectData)
                {
                    if (eff == null) continue;
                    var clone = eff.Clone(apply);
                    inst.rolledEffects.Add(clone);
                }
            }

            // Determine tier when the card's variability rules expect a single effect
            if (source.IsCardVariabilityValid() && inst.rolledEffects.Count == 1)
            {
                inst.tier = source.GetVariationTier(inst.rolledEffects[0]);
            }

            return inst;
        }

        /// <summary>
        /// Returns the total rolled value for effects of the given operation type.
        /// </summary>
        public int GetTotal(OperationType operation)
        {
            int sum = 0;
            foreach (var e in rolledEffects)
            {
                if (e != null && e.operationType == operation)
                {
                    sum += e.postCopyValue;
                }
            }
            return sum;
        }
    }
}
