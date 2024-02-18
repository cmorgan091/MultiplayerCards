using MultiplayerCards.Domain;
using MultiplayerCards.Domain.Games.Snap;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiplayerCards.MPCMD
{
    class Program
    {
        static void Main(string[] args)
        {
            //// start by just outputting all the cards in the deck
            //var deck = CardDeckFactory.CreateStandardDeck();

            //Console.WriteLine($"Listing all {deck.Cards.Count} cards in the deck");

            //Console.WriteLine(string.Join(", ", deck.Cards));

            //Console.WriteLine($"Shuffling deck and listing all cards");

            //Console.WriteLine(string.Join(", ", deck.GetShuffledCards()));

            //Console.WriteLine($"Creating a game of snap with two players");

            //var players = new List<Player>
            //{
            //    new Player("Player 1", true),
            //    new Player("Player 2", true),
            //};

            //var game = new Game(new GameDefinition(), players, deck, new GameTable());

            //// deal the cards
            //game.Start();

            //// take a look at the state
            //Console.WriteLine($"State after dealing");

            //Console.WriteLine(game.ToDebugString());

            // new methodology
            var game = new SnapGame();

            game.InitialiseGame(new SnapGameOptions
            {
                AutoStartWhenMinPlayersReached = false,
            });

            game.JoinGame(new Player("Player 1", true));
            game.JoinGame(new Player("Player 2", true));

            
            game.StartGame();

            while (game.Status == GameStatus.Playing)
            {
                // monitoring thread
                Thread.Sleep(1000);
            }
        }
    }
}
