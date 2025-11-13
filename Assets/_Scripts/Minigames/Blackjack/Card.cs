using System;


namespace Blackjack
{
    [Serializable]
    public enum Suit { Clubs, Diamonds, Hearts, Spades }


    [Serializable]
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack = 10, Queen = 10, King = 10, Ace = 11 }


    [Serializable]
    public struct Card
    {
        public Suit Suit;
        public int Value; // 2..11 (Ace as 11 by default)
        public string Face; // "2".."10","J","Q","K","A"


        public Card(Suit suit, int value, string face)
        {
            Suit = suit;
            Value = value;
            Face = face;
        }


        public override string ToString() => $"{Face} of {Suit}";
    }
}