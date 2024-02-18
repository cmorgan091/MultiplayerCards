using System;
using System.Collections.Generic;
using System.Linq;

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

        public void MoveFirstTo(CardSet targetCardSet)
        {
            var card = this.FirstOrDefault();

            if (card == null)
            {
                throw new Exception($"Cannot {nameof(MoveFirstTo)} as there are no cards in this card set");
            }

            // remove from this card set and add to another card set
            Remove(card);
            targetCardSet.Add(card);
        }

        public void MoveAllTo(CardSet targetCardSet)
        {
            targetCardSet.AddRange(this);
            RemoveAll(x => x != null);
        }

        public string ToDebugString()
        {
            return $"{Name} ({State}) ({this.Count()}) - {string.Join(", ", this.Select(x => x.ToString()))}";
        }
    }
}
