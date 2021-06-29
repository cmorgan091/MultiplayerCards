using System.Text;

namespace MultiplayerCards.Domain
{
    /// <summary>
    /// A GameTable is a place for shared resources to interact, e.g. CardSets
    /// </summary>
    public class GameTable
    {
        public CardSet DiscardPile { get; } = new CardSet("Discard Pile", CardSetStates.Blind);

        public CardSet DrawPile { get; } = new CardSet("Draw Pile", CardSetStates.Blind);

        public CardSet PlayPile { get; } = new CardSet("Play Pile", CardSetStates.OnTable);

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Table:");
            sb.AppendLine($"   {DiscardPile.ToDebugString()}");
            sb.AppendLine($"   {DrawPile.ToDebugString()}");
            sb.AppendLine($"   {PlayPile.ToDebugString()}");

            return sb.ToString();
        }
    }
}
