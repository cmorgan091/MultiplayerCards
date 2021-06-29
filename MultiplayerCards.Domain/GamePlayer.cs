using System.Collections.Generic;
using System.Linq;

namespace MultiplayerCards.Domain
{
    /// <summary>
    /// A GamePlayer is a player taking part in a game of cards, and will own one or more CardSet's
    /// </summary>
    public class GamePlayer
    {
        public GamePlayer(Game game, Player player, List<CardSet> cardSets)
        {
            Game = game;
            Player = player;
            CardSets = cardSets;
        }

        public Game Game { get; }

        public Player Player { get; }

        public List<CardSet> CardSets { get; }

        public bool IsPlayersTurn { get; private set; }

        public void StartTurn()
        {
            IsPlayersTurn = true;

            // if the player has any cards, then they lay it
            if (CardSets[0].Any())
            {
                CardSets[0].MoveFirstTo(Game.Table.PlayPile);
            }

            EndTurn();
        }

        public void EndTurn()
        {
            IsPlayersTurn = false;

            Game.TurnComplete();
        }

        public override string ToString()
        {
            return Player.ToString();
        }
    }
}
