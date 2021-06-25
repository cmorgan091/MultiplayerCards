using System;
using System.Collections.Generic;

namespace MultiplayerCards.Domain
{
    /// <summary>
    /// A CardSet is a set of cards usually held by a player, card sets have properties, e.g. in hand, or on table, or blind
    /// </summary>
    public class CardSet : List<Card>
    {
        public CardSet(string name, CardSetStates state)
        {
            Name = name;
            State = state;
        }

        public string Name { get; }

        public CardSetStates State { get; }
    }
}
