using System.Collections.Generic;
using System.Linq;

namespace MultiplayerCards.Domain
{
    public class Deck
    {
        public Deck(IEnumerable<Card> cards)
        {
            Cards = cards.ToList();
        }

        public List<Card> Cards { get; set; }
    }
}
