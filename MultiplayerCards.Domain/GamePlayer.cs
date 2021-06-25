using System.Collections.Generic;

namespace MultiplayerCards.Domain
{
    /// <summary>
    /// A GamePlayer is a player taking part in a game of cards, and will own one or more CardSet's
    /// </summary>
    public class GamePlayer
    {
        public GamePlayer(Player player, List<CardSet> cardSets)
        {
            Player = player;
            CardSets = cardSets;
        }

        public Player Player { get; }

        public List<CardSet> CardSets { get; }

        public override string ToString()
        {
            return Player.ToString();
        }
    }
}
