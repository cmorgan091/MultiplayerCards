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
            => Cards.Shuffle();
    }

    public static class CardExtensions
    { 
        public static IOrderedEnumerable<Card> Shuffle(this IEnumerable<Card> cards)
        {
            var random = new Random();
            return cards.OrderBy(c => random.Next());
        }
    }
}
