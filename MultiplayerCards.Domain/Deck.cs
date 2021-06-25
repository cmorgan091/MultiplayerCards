using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerCards.Domain
{
    public class Deck
    {
        public Deck(IEnumerable<Card> cards)
        {
            Cards = cards.ToList();
        }

        public List<Card> Cards { get; set; }

        public IOrderedEnumerable<Card> GetShuffledCards()
        {
            var random = new Random();
            return Cards.OrderBy(c => random.Next());
        }
    }
}
