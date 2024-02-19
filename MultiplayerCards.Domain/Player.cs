using MultiplayerCards.Domain.Games.Snap;
using System;
using System.Threading.Tasks;

namespace MultiplayerCards.Domain
{
    /// <summary>
    /// A player takes place in the card games
    /// </summary>
    public class Player
    {
        private IGamePlayer currentGamePlayer;

        public Player(string name, bool isCpu)
        {
            Name = name;
            IsCpu = isCpu;
        }

        public bool IsPlayingGame => currentGamePlayer != null;

        public void LinkToGamePlayer(IGamePlayer gamePlayer)
        {
            if (currentGamePlayer != null)
            {
                throw new Exception($"Cannot {nameof(LinkToGamePlayer)} as already linked");
            }

            currentGamePlayer = gamePlayer;
        }

        public void StartGamePlayingLoop()
        {
            if (currentGamePlayer == null)
            {
                throw new Exception($"Cannot {nameof(StartGamePlayingLoop)} as no game player has been linked");
            }

            currentGamePlayer.StartGamePlayingLoopAsync();
        }

        public void StopGamePlayingLoop()
        {
            if (currentGamePlayer == null)
            {
                throw new Exception($"Cannot {nameof(StopGamePlayingLoop)} as no game player has been linked");
            }

            currentGamePlayer.StopGamePlayingLoop();
        }

        public string Name { get; }

        public bool IsCpu { get; }

        public override string ToString()
        {
            return Name + (IsCpu ? " (CPU)" : "");
        }
    }
}
