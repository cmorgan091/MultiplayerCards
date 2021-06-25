using MultiplayerCards.Domain;
using System;

namespace MultiplayerCards.MPCMD
{
    class Program
    {
        static void Main(string[] args)
        {
            // start by just outputting all the cards in the deck
            var deck = CardDeckFactory.CreateStandardDeck();

            Console.WriteLine($"Listing all {deck.Cards.Count} cards in the deck");

            Console.WriteLine(string.Join(", ", deck.Cards));
        }
    }
}
