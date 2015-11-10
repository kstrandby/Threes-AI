using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threes_console
{
    public class Deck
    {
        private List<int> cards;

        public Deck()
        {
            cards = new List<int> {1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3};
            this.Shuffle();
        }

        public Deck(Deck deck)
        {
            this.cards = new List<int>(deck.cards);
        }

        public int PeekNextCard()
        {
            return cards[cards.Count - 1];
        }

        public int DealCard()
        {
            int card = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            if (cards.Count == 0) GenerateNewDeck();
            return card;
        }

        // Shuffle method based on Fisher-Yates
        public void Shuffle()
        {
            Random random = new Random();
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }  
        }

        private void GenerateNewDeck()
        {
            cards = new List<int> { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3 };
            this.Shuffle();
        }

        public void Remove(int card)
        {
            this.cards.Remove(card);
        }

        public void Add(int card)
        {
            this.cards.Add(card);
        }

        public bool IsEmpty()
        {
            if (this.cards.Count == 0) return true;
            else return false;
        }
        public List<int> GetAllCards()
        {
            return this.cards;
        }

        public Deck Clone()
        {
            return new Deck(this);
        }

        public void PrintCardsLeft()
        {
            String toPrint = "";
            foreach (int card in cards.Distinct())
            {
                toPrint += card + " ";
            }
            Console.WriteLine(toPrint);
        }
    }
}
