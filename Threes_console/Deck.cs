using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threes_console
{
    // Class to represent a card deck
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

        // returns the value of the next card coming up
        public int PeekNextCard()
        {
            return cards[cards.Count - 1];
        }

        // returns and removes the value of the next card coming up
        public int DealCard()
        {
            int card = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            if (cards.Count == 0) GenerateNewDeck();
            return card;
        }

        // Shuffles the list of cards
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

        // Generates a new deck
        private void GenerateNewDeck()
        {
            cards = new List<int> { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3 };
            this.Shuffle();
        }

        // Removes a card with a certain value
        public void Remove(int card)
        {
            this.cards.Remove(card);
        }

        // Adds a card with a certain value
        public void Add(int card)
        {
            this.cards.Add(card);
        }
        
        // Checks if the card deck is empty
        public bool IsEmpty()
        {
            if (this.cards.Count == 0) return true;
            else return false;
        }

        // Returns all cards in the deck
        public List<int> GetAllCards()
        {
            return this.cards;
        }

        // Clones the deck
        public Deck Clone()
        {
            return new Deck(this);
        }

        // Prints the value of the cards in the deck to the console
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
