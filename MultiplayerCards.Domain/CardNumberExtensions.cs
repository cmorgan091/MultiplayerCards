namespace MultiplayerCards.Domain
{
    public static class CardNumberExtensions
    {
        public static string ToShortName(this CardNumbers number)
        {
            if (number > CardNumbers.Ace && number < CardNumbers.Jack)
            {
                return ((int)number).ToString();
            }

            return number.ToString().Substring(0, 1);
        }
    }
}
