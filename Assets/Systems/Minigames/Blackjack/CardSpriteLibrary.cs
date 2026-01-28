using System;
using System.Collections.Generic;
using UnityEngine;
using Blackjack; // for Suit enum

[CreateAssetMenu(menuName = "Blackjack/Card Sprite Library")]
public class CardSpriteLibrary : ScriptableObject
{
    [Serializable]
    public class SuitSprites
    {
        public Suit suit;
        // 13 sprites in the order: 2,3,4,5,6,7,8,9,10,J,Q,K,A
        public Sprite[] ranks = new Sprite[13];
    }

    public SuitSprites[] suits;
    public Sprite cardBack; // for hidden dealer card

    static readonly string[] Faces = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    Dictionary<(Suit, int), Sprite> map;

    void OnEnable()
    {
        map = new Dictionary<(Suit, int), Sprite>();
        foreach (var ss in suits)
            for (int i = 0; i < ss.ranks.Length; i++)
                if (ss.ranks[i] != null)
                    map[(ss.suit, i)] = ss.ranks[i];
    }

    int FaceToIndex(string face)
    {
        for (int i = 0; i < Faces.Length; i++)
            if (Faces[i] == face) return i;
        return -1;
    }

    public Sprite GetCardSprite(Suit suit, string face)
    {
        int idx = FaceToIndex(face);
        if (idx < 0) return null;
        return map.TryGetValue((suit, idx), out var sp) ? sp : null;
    }
}
