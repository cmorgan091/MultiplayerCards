namespace MultiplayerCards.Domain
{
    public static class CardSuitExtensions
    {
        public static char ToChar(this CardSuits suit, bool unicode = false)
        {
            if (unicode)
            {
                switch (suit)
                {
                    case CardSuits.Club:
                        return '\u2663';
                    case CardSuits.Diamond:
                        return '\u2666';
                    case CardSuits.Spade:
                        return '\u2660';
                    case CardSuits.Heart:
                        return '\u2665';
                }
            }

            return suit.ToString()[0];
        }
    }
}
