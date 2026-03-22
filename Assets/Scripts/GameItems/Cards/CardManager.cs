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
            List<CardData> result = new();

            // please do not change the uniqueID for the cards
            // TODO: auto-populate these runtime with the actual uniqueIDs

            // BASE CARDS
            const int BLOCK_ID = 1;
            const int HEAL_POTION_ID = 6;
            const int SLASH_ID = 9;

            // SPECIAL CARDS
            const int DOUBLE_SLASH_ID = 2;
            const int DRAMATIC_EXIT_ID = 3;
            const int EXCHANGE_ID = 4;
            const int TARIFF_STRIKE_ID = 5;
            const int ENERGY_DRINK_ID = 7;
            const int SHIELD_SLASH_ID = 8;
            const int WORKOUT_ID = 10;

            /*
            process:
            1) pull every special card into result array, one of each
            2) pull guaranteed minimum of the base cards
            3) pull a random 'number' amount of cards in unlockedCardPool excluding certain cards
            */

            // 1) Exclude base cards using PullCard() to get CardData versions
            List<CardData> excludeBaseCards = new()
            {
                PullCard(BLOCK_ID),
                PullCard(HEAL_POTION_ID),
                PullCard(SLASH_ID)
            };
            excludeBaseCards.RemoveAll(c => c == null); // clean up nulls

            // Get all special cards (all unlocked, except the base cards)
            List<CardData> specialCardPool =
                allCardPool.FindAll(c => IsCardUnlocked(c, excludeBaseCards));

            // Add exactly 1 copy of each special card
            foreach (var card in specialCardPool)
            {
                if (card != null)
                    result.Add(card);
            }

            // 2)

            const int MIN_SLASH = 12;
            const int MIN_BLOCK = 3;
            const int MIN_HEAL = 6;

            CardData slash = PullCard(SLASH_ID);
            if (slash != null) {
                for (int i = 0; i < MIN_SLASH; i++) {
                    result.Add(slash);
                }
            }

            CardData block = PullCard(BLOCK_ID);
            if (block != null) {
                for (int i = 0; i < MIN_BLOCK; i++) {
                    result.Add(block);
                }
            }

            CardData heal = PullCard(HEAL_POTION_ID);
            if (heal != null) {
                for (int i = 0; i < MIN_HEAL; i++) {
                    result.Add(heal);
                }
            }

            // Optional: reduce the amount of random cards so total stays consistent
            number -= result.Count;
            if (number < 0) number = 0;

            // 3)
            List<CardData> unlockedPool = allCardPool.FindAll(c => IsCardUnlocked(c, specialCardPool)); // for now, only allow one instance of specialcards
            if (unlockedPool.Count == 0)
            {
                Debug.LogWarning("No unlocked cards available!");
                return result;
            }

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

            PileCountUI.RefreshNow();
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
            PileCountUI.RefreshNow();
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
            PileCountUI.RefreshNow();
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
            PileCountUI.RefreshNow();
            return removed;
        }

        // Valid card pull checker depending on flag
        private bool IsCardUnlocked(CardData card, List<CardData> cardExclude = null)
        {

            cardExclude ??= new List<CardData>(); // empty list if field isn't filled
            
            if (card == null) return false;

            // Always unlocked if card is marked default
            // also as a fallback for the three main default cards
            // if (card.unlockedByDefault)
            //     return true;

            // No unlock flag? Treat as unlocked
            // if (string.IsNullOrEmpty(card.unlockFlag))
            //     return true;

            // Check if card has unlock flag, and not in uniqueIDExclude list
            return (GameFlags.HasFlag(card.unlockFlag) && !cardExclude.Contains(card));
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
