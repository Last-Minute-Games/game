using System.Collections.Generic;


namespace Blackjack
{
    public class Deck
    {
        private readonly Stack<Card> _stack = new Stack<Card>();
        private static readonly string[] Faces = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
        private static readonly int[] Values = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 11 };


        public int Count => _stack.Count;


        public Deck(int numberOfDecks = 1)
        {
            Rebuild(numberOfDecks);
        }


        public void Rebuild(int numberOfDecks = 1)
        {
            var list = new List<Card>(52 * numberOfDecks);
            for (int d = 0; d < numberOfDecks; d++)
            {
                foreach (Suit s in System.Enum.GetValues(typeof(Suit)))
                {
                    for (int i = 0; i < Faces.Length; i++)
                    {
                        list.Add(new Card(s, Values[i], Faces[i]));
                    }
                }
            }
            Shuffle(list);
            _stack.Clear();
            for (int i = 0; i < list.Count; i++) _stack.Push(list[i]);
        }


        private static void Shuffle(List<Card> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }


        public Card Draw()
        {
            if (_stack.Count == 0) Rebuild(1);
            return _stack.Pop();
        }
    }
}