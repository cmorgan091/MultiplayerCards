using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiplayerCards.Domain
{
    public class Game
    {
        public Game(GameDefinition definition, List<Player> players, Deck deck)
        {
            Definition = definition;
            GamePlayers = players.Select(p => new GamePlayer(p, new List<CardSet> { new CardSet("Hand", CardSetStates.InHand) })).ToList();
            Deck = deck;
        }

        public void Start()
        {
            // deal the cards
            var cards = Deck.GetShuffledCards();

            var currentGamePlayerId = 0;
            foreach (var card in cards)
            {
                // put card in players hand
                var gamePlayer = GamePlayers[currentGamePlayerId];
                gamePlayer.CardSets[0].Add(card);

                currentGamePlayerId++;
                if (currentGamePlayerId == GamePlayers.Count)
                {
                    currentGamePlayerId = 0;
                }
            }
        }

        public GameDefinition Definition { get; }

        public List<GamePlayer> GamePlayers { get; }

        public Deck Deck { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            foreach (var player in GamePlayers)
            {
                sb.AppendLine($"Player: {player}");
                sb.AppendLine($"   Card Sets:");
                foreach (var cardSet in player.CardSets)
                {
                    sb.AppendLine($"      {cardSet.State} ({cardSet.Count()}) - {string.Join(", ", cardSet.Select(x => x.ToString()))}");
                }
            }

            return sb.ToString();
        }
    }
}
