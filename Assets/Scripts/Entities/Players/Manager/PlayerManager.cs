
using System;
using System.Collections.Generic;

public struct PlayerManager
{
    public int health;
    public int maxHealth;
    public int energy;
    public int maxEnergy;
    public int block;

    public List<CardData> drawPile;
    public List<CardData> discardPile;
    public List<CardData> hand;

    public void StartDeck(List<CardData> startingDeck)
    {
        drawPile = new List<CardData>(startingDeck);
        hand = new List<CardData>();
        discardPile = new List<CardData>();
        Shuffle(drawPile);
    }

    public void StartTurn()
    {
        energy = maxEnergy;
        
    }
    
    private void Shuffle(List<CardData> pile)
    {
        var rand = new Random();
        for (int i = 0; i < pile.Count; i++)
        {
            int j = rand.Next(pile.Count);
            (pile[i], pile[j]) = (pile[j], pile[i]);
        }
    }
}