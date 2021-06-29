using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiplayerCards.Domain
{
    public class Game
    {
        public Game(GameDefinition definition, List<Player> players, Deck deck, GameTable table)
        {
            Definition = definition;
            GamePlayers = players.Select(p => new GamePlayer(this, p, new List<CardSet> { new CardSet("Blind", CardSetStates.Blind) })).ToList();
            Deck = deck;
            Table = table;
        }

        public void Start()
        {
            // deal the cards
            var cards = Deck.GetShuffledCards();

            var currentGamePlayerId = 0;
            foreach (var card in cards)
            {
                // put card in players blind hand
                var gamePlayer = GamePlayers[currentGamePlayerId];
                gamePlayer.CardSets[0].Add(card);

                currentGamePlayerId++;
                if (currentGamePlayerId == GamePlayers.Count)
                {
                    currentGamePlayerId = 0;
                }
            }

            // the first player dealt to goes first
            CurrentPlayerTurnId = 0;
            GamePlayers[CurrentPlayerTurnId].StartTurn();
        }

        public void TurnComplete()
        {
            // occurs every time a player completes a turn


            // increment the player turn id
            CurrentPlayerTurnId++;
            if (CurrentPlayerTurnId == GamePlayers.Count)
            {
                CurrentPlayerTurnId = 0;
            }


        }

        public GameDefinition Definition { get; }

        public List<GamePlayer> GamePlayers { get; }

        public Deck Deck { get; }

        public GameTable Table { get; }

        public int CurrentPlayerTurnId { get; private set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            foreach (var player in GamePlayers)
            {
                sb.AppendLine($"Player: {player}");
                sb.AppendLine($"   Card Sets:");
                foreach (var cardSet in player.CardSets)
                {
                    sb.AppendLine($"      {cardSet.ToDebugString()}");
                }
            }

            sb.AppendLine($"{Table.ToDebugString()}");

            return sb.ToString();
        }
    }
}
