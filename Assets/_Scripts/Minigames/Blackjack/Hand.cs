using System.Collections.Generic;


namespace Blackjack
{
    public class Hand
    {
        public readonly List<Card> Cards = new List<Card>();
        public int SoftAces { get; private set; } // Aces currently counted as 11


        public void Clear()
        {
            Cards.Clear();
            SoftAces = 0;
        }


        public void Add(Card c)
        {
            Cards.Add(c);
            if (c.Value == 11) SoftAces++;
        }


        public int Total()
        {
            int sum = 0;
            int soft = SoftAces;
            foreach (var c in Cards) sum += c.Value;
            // Downgrade Aces from 11 to 1 while busting
            while (sum > 21 && soft > 0)
            {
                sum -= 10; // 11 -> 1
                soft--;
            }
            return sum;
        }


        public bool IsBlackjack() => Cards.Count == 2 && Total() == 21;
        public bool IsBust() => Total() > 21;
    }
}