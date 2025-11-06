using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardManager
{
    [Header("Card Pools")]
    public List<CardData> allCardPool = new();     // All available cards in the game
    public List<CardData> drawPile = new();        // Current draw pile
    public List<CardData> hand = new();            // Cards currently in hand
    public List<CardData> discardPile = new();     // Discarded cards

    private System.Random rng = new();

    // -----------------------------------------------------------
    // Generate a set of random cards (e.g., for initial deck, rewards, etc.)
    // -----------------------------------------------------------
    public List<CardData> GenerateRandomCards(int number)
    {
        List<CardData> result = new();
        if (allCardPool.Count == 0)
        {
            Debug.LogWarning("CardManager: No cards available in allCardPool.");
            return result;
        }

        for (int i = 0; i < number; i++)
        {
            var card = allCardPool[rng.Next(allCardPool.Count)];
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
            if (card.UniqueID == uniqueID)
                return card;
        }

        Debug.LogWarning($"CardManager: Card with UniqueID {uniqueID} not found.");
        return null;
    }

    // -----------------------------------------------------------
    // Move all cards from hand to discard pile (e.g., at end of turn)
    // -----------------------------------------------------------
    public void DiscardCardPile()
    {
        discardPile.AddRange(hand);
        hand.Clear();
    }

    // -----------------------------------------------------------
    // Apply the effects of a given card to a target entity
    // -----------------------------------------------------------
    public void ApplyCard(CardData card, EntityData source, ref EntityData target)
    {
        if (card == null) return;

        foreach (var effect in card.effectDataList)
        {
            switch (effect.effectType)
            {
                case EffectType.Damage:
                    target.TakeDamage(effect.magnitude);
                    break;

                case EffectType.Block:
                    source.GainBlock(effect.magnitude);
                    break;

                case EffectType.Heal:
                    source.Heal(effect.magnitude);
                    break;

                case EffectType.Draw:
                    // Card draw logic handled by PlayerManager
                    Debug.Log($"{source.name} would draw {effect.magnitude} cards.");
                    break;

                case EffectType.ApplyStatus:
                    target.statuses.Add(new StatusEffect
                    {
                        name = "Status",
                        stacks = effect.magnitude
                    });
                    break;
            }
        }

        // Play sound cue if assigned
        // if (card.SoundCue != null && card.SoundCue.Clip != null)
        // {
        //     AudioSource.PlayClipAtPoint(card.SoundCue.Clip, Vector3.zero, card.SoundCue.Volume);
        // }

        // Move card to discard pile
        hand.Remove(card);
        discardPile.Add(card);
    }

    // -----------------------------------------------------------
    // Shuffle the draw pile (utility)
    // -----------------------------------------------------------
    public void ShuffleDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int swap = rng.Next(drawPile.Count);
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
    }

    private void ReshuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
    }
}
