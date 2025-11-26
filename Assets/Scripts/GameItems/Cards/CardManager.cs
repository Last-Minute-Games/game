namespace GameItems.Cards
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class CardManager
    {
        [Header("Card Pools")]
        public List<CardData> allCardPool = new();     // All available cards in the game
        public List<CardData> drawPile = new();        // Current draw pile
        public List<CardData> hand = new();            // Cards currently in hand
        public List<CardData> discardPile = new();     // Discarded cards

        [Header("Runtime (Rolled Instances)")]
        public List<CardInstance> handInstances = new();

        private System.Random _rng = new();

        // -----------------------------------------------------------
        // Generate a set of random cards (e.g., for initial deck, rewards, etc.)
        // -----------------------------------------------------------
        public List<CardData> GenerateRandomCards(int number)
        {
            List<CardData> unlockedPool = allCardPool.FindAll(c => IsCardUnlocked(c)); // filter unlocked cards

            if (unlockedPool.Count == 0)
            {
                Debug.LogWarning("No unlocked cards available!");
                return new();
            }

            List<CardData> result = new();
            for (int i = 0; i < number; i++)
            {
                var card = unlockedPool[_rng.Next(unlockedPool.Count)];
                result.Add(card);
            }

            return result;
        }

        // -----------------------------------------------------------
        // Retrieve a card by its UniqueID from the global pool
        // -----------------------------------------------------------
        public CardData PullCard(int uniqueID)
        {
            foreach (var card in allCardPool)
            {
                if (card.uniqueID == uniqueID)
                    return card;
            }

            Debug.LogWarning($"CardManager: Card with UniqueID {uniqueID} not found.");
            return null;
        }

        // -----------------------------------------------------------
        // Shuffle the draw pile (utility)
        // -----------------------------------------------------------
        public void ShuffleDrawPile()
        {
            for (int i = 0; i < drawPile.Count; i++)
            {
                int swap = _rng.Next(drawPile.Count);
                (drawPile[i], drawPile[swap]) = (drawPile[swap], drawPile[i]);
            }
        }

        // -----------------------------------------------------------
        // Draw a card from the draw pile to the hand
        // -----------------------------------------------------------
        public void DrawCard()
        {
            if (drawPile.Count == 0)
            {
                ReshuffleDiscardIntoDraw();
                if (drawPile.Count == 0)
                {
                    Debug.Log("No cards left to draw.");
                    return;
                }
            }

            var card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);

            // Create a runtime instance with rolled values for this draw
            var instance = CardInstance.FromData(card, applyVariability: true);
            if (instance != null)
            {
                handInstances.Add(instance);
            }
        }

        public CardInstance GetLatestInstanceFor(CardData data)
        {
            for (int i = handInstances.Count - 1; i >= 0; i--)
            {
                if (handInstances[i] != null && handInstances[i].data == data)
                    return handInstances[i];
            }
            return null;
        }

        /// <summary>
        /// Returns the rolled total for a given operation on the most recent instance of the specified card.
        /// Falls back to summing base values on CardData if no instance exists.
        /// </summary>
        public int GetRolledTotal(CardData data, OperationType op)
        {
            var inst = GetLatestInstanceFor(data);
            if (inst != null)
            {
                return inst.GetTotal(op);
            }

            int sum = 0;
            if (data != null && data.effects != null)
            {
                foreach (var e in data.effects)
                {
                    if (e.operationType == op)
                        sum += e.baseValue;
                }
            }
            return sum;
        }

        // -----------------------------------------------------------
        // Move all cards from hand to discard pile (e.g., at end of turn)
        // -----------------------------------------------------------
        public void DiscardCardPile()
        {
            discardPile.AddRange(hand);
            hand.Clear();
            handInstances.Clear();
        }

        // -----------------------------------------------------------
        // Clear discard pile (e.g., for new wave/battle)
        // -----------------------------------------------------------
        public void ClearDiscardPile()
        {
            discardPile.Clear();
            Debug.Log("[CardManager] Discard pile cleared");
        }

        // -----------------------------------------------------------
        // Draw a starting hand (typically 5 cards)
        // -----------------------------------------------------------
        public void DrawStartingHand(int count = 5)
        {
            for (int i = 0; i < count; i++)
            {
                DrawCard();
            }
            Debug.Log($"[CardManager] Drew starting hand of {count} cards");
        }

        private void ReshuffleDiscardIntoDraw()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDrawPile();
        }

        // -----------------------------------------------------------
        // Move a specific card from hand to discard pile and remove its instance
        // -----------------------------------------------------------
        public bool PlayCardFromHand(CardData data, CardInstance instance = null)
        {
            if (data == null)
            {
                Debug.LogWarning("[CardManager] PlayCardFromHand called with null CardData");
                return false;
            }

            // Remove the card from hand
            bool removed = hand.Remove(data);
            if (!removed)
            {
                Debug.LogWarning($"[CardManager] Card '{data.name}' not found in hand when trying to play it.");
            }

            // Add to discard pile regardless so we don't lose track of it
            discardPile.Add(data);

            // Remove the corresponding instance if provided or infer by data
            if (instance != null)
            {
                handInstances.Remove(instance);
            }
            else
            {
                // Fallback: remove the most recent instance for this data
                for (int i = handInstances.Count - 1; i >= 0; i--)
                {
                    if (handInstances[i] != null && handInstances[i].data == data)
                    {
                        handInstances.RemoveAt(i);
                        break;
                    }
                }
            }

            Debug.Log($"[CardManager] Played card '{data.name}'. Hand: {hand.Count}, Discard: {discardPile.Count}, Instances: {handInstances.Count}");
            return removed;
        }

        // Valid card pull checker depending on flag
        private bool IsCardUnlocked(CardData card)
        {
            if (card == null) return false;

            // Always unlocked if card is marked default
            // also as a fallback for the three main default cards
            if (card.unlockedByDefault)
                return true;

            // No unlock flag? Treat as unlocked
            if (string.IsNullOrEmpty(card.unlockFlag))
                return true;

            // Otherwise check the flag system
            return GameFlags.HasFlag(card.unlockFlag);
        }

        // // -----------------------------------------------------------
        // // Apply the effects of a given card to a target entity
        // // -----------------------------------------------------------
        // public void ApplyCard(CardData card, EntityData source, ref EntityData target)
        // {
        //     if (card == null) return;
    //
        //     foreach (var effect in card.effectDataList)
        //     {
        //         switch (effect.effectType)
        //         {
        //             case EffectType.Damage:
        //                 target.TakeDamage(effect.magnitude);
        //                 break;
    //
        //             case EffectType.Block:
        //                 source.GainBlock(effect.magnitude);
        //                 break;
    //
        //             case EffectType.Heal:
        //                 source.Heal(effect.magnitude);
        //                 break;
    //
        //             case EffectType.Draw:
        //                 // Card draw logic handled by PlayerManager
        //                 Debug.Log($"{source.name} would draw {effect.magnitude} cards.");
        //                 break;
    //
        //             case EffectType.ApplyStatus:
        //                 target.statuses.Add(new StatusEffect
        //                 {
        //                     name = "Status",
        //                     stacks = effect.magnitude
        //                 });
        //                 break;
        //         }
        //     }
    //
        //     // Play sound cue if assigned
        //     // if (card.SoundCue != null && card.SoundCue.Clip != null)
        //     // {
        //     //     AudioSource.PlayClipAtPoint(card.SoundCue.Clip, Vector3.zero, card.SoundCue.Volume);
        //     // }
    //
        //     // Move card to discard pile
        //     hand.Remove(card);
        //     discardPile.Add(card);
        // }
    }
}
