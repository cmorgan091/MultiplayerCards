﻿namespace MultiplayerCards.Domain
{
    public class Card
    {
        public Card(CardNumbers number, CardSuits suit)
        {
            Number = number;
            Suit = suit;
        }

        public CardNumbers Number { get; set; }
        
        public CardSuits Suit { get; set; }

        public override string ToString()
        {
            return $"{Suit.ToChar(true)}{Number.ToShortName()}";
        }
    }
}
