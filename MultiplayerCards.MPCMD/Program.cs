using MultiplayerCards.Domain;
using MultiplayerCards.Domain.Games.Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            

            var player1 = new CpuPlayer("High IQ, medium speed", CpuIntelligence.High, CpuReactions.Medium);
            var player2 = new CpuPlayer("Low IQ, fast speed", CpuIntelligence.Low, CpuReactions.Fast);

            var results = new List<GameResult>();

            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine($"-- Starting game {i + 1} --");

                var game = new SnapGame();

                game.InitialiseGame(new SnapGameOptions
                {
                    AutoStartWhenMinPlayersReached = false,
                });

                var player1Response = game.JoinGame(player1);
                var player2Response = game.JoinGame(player2);

                var task1 = Task.Run(() => player1.StartGamePlayingLoop());
                var task2 = Task.Run(() => player2.StartGamePlayingLoop());

                game.StartGame();

                while (game.Status == GameStatus.Playing)
                {
                    // monitoring thread
                    Thread.Sleep(1000);
                }

                var response = game.CloseGame();

                Console.WriteLine($"-- Closing game {i + 1} --");

                results.Add(response);
            }

            var players = new List<Player> { player1, player2 };

            Console.WriteLine("| Player                    | Wins | Draws | Losses |");
            Console.WriteLine("|---------------------------|------|-------|--------|");
            foreach (var player in players)
            {
                var wins = results?.Count(x => x.Winners?.Contains(player) ?? false) ?? 0;
                var draws = results?.Count(x => x.Drawers?.Contains(player) ?? false) ?? 0;
                var losses = results?.Count(x => x.Losers?.Contains(player) ?? false) ?? 0;

                Console.WriteLine($"| {player.Name,25} | {wins,4} | {draws,5} | {losses,6} |");
            }
        }
            
    }
}
