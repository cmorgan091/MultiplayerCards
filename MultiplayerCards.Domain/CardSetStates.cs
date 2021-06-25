namespace MultiplayerCards.Domain
{
    public enum CardSetStates
    {
        InHand, // visible only to the player
        OnTable, // visible to all
        Blind, // invisible to all
    }
}
