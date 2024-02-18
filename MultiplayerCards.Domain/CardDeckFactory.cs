using System;
using System.Collections.Generic;

namespace MultiplayerCards.Domain
{
    public static class CardDeckFactory
    {
        public static Deck CreateStandardDeck()
        {
            var cards = new List<Card>();

            foreach (var suit in Enum.GetValues<CardSuits>())
            {
                foreach (var number in Enum.GetValues<CardNumbers>())
                {
                    cards.Add(new Card(number, suit));
                }
            }

            var id = 0;
            foreach (var card in cards.Shuffle())
            {
                card.Id = id;
                id++;
            };

            return new Deck(cards);
        }
    }
}
