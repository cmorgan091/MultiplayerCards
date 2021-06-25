namespace MultiplayerCards.Domain
{
    /// <summary>
    /// A player takes place in the card games
    /// </summary>
    public class Player
    {
        public Player(string name, bool isCpu)
        {
            Name = name;
            IsCpu = isCpu;
        }

        public string Name { get; }

        public bool IsCpu { get; }

        public override string ToString()
        {
            return Name + (IsCpu ? " (CPU)" : "");
        }
    }
}
