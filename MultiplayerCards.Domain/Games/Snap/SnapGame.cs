using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerCards.Domain.Games.Snap
{
    /// <summary>
    /// The Snap Game itself
    /// This is executing on the server and controls every aspect of the game
    /// </summary>
    public class SnapGame : IGame
    {
        public string Name { get; } = "Snap";
        public int MinPlayers { get; } = 2;
        public int MaxPlayers { get; } = 2;
        public GameStatus Status { get; private set; } = GameStatus.NotInitialised;

        private SnapGameOptions Options;
        private List<SnapGamePlayer> Players = new();
        private Deck Deck;

        private Dictionary<SnapGamePlayer, CardSet> PlayerCardStacks;
        private Dictionary<SnapGamePlayer, int> PlayerHandsWon;
        private CardSet InPlayStack;
        private List<SnapGameState> GameStatesList = new List<SnapGameState>();
        private SnapGamePlayer CurrentPlayersTurn;

        public void InitialiseGame(SnapGameOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(Options));

            if (Status != GameStatus.NotInitialised)
            {
                throw new Exception($"Cannot {nameof(InitialiseGame)} as game status is {Status}");
            }

            Options = options;
            Status = GameStatus.ReadyToStart;
        }

        /// <summary>
        /// Player request to join game
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public JoinGameResponse JoinGame(Player player)
        {
            // boiler plate stuff that happens for every game
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (Status != GameStatus.WaitingForPlayers && Status != GameStatus.ReadyToStart)
            {
                // not in the right state for a player to join
                return new JoinGameResponse { Success = false, FailureReason = $"Game is in status {Status} which does not allow new players to join" };
            }

            if (Players.Count >= MaxPlayers)
            {
                // already reached max number of players
                return new JoinGameResponse { Success = false, FailureReason = $"Game already has the maximum number of players ({MaxPlayers})" };
            }

            // ok so we can add this player
            var gamePlayer = new SnapGamePlayer(this, player);
            player.LinkToGamePlayer(gamePlayer);

            Players.Add(gamePlayer);

            if (Players.Count >= MinPlayers)
            {
                Status = GameStatus.ReadyToStart;

                if (Options.AutoStartWhenMinPlayersReached)
                {
                    StartGame();
                }
            }

            return new JoinGameResponse { Success = true, GamePlayer = gamePlayer };
        }

        public void StartGame()
        {
            if (Status != GameStatus.ReadyToStart)
            {
                throw new Exception($"Cannot {nameof(StartGame)} because game status is not {nameof(GameStatus.ReadyToStart)} ({Status})");
            }

            if (Players.Count < MinPlayers || Players.Count > MaxPlayers)
            {
                throw new Exception($"Cannot {nameof(StartGame)} because number of players is {Players.Count}");
            }

            Status = GameStatus.Playing;

            // so to setup a game of snap, we get a shuffled deck of cards deal all but one card to the players
            Deck = CardDeckFactory.CreateStandardDeck();
            var allCards = Deck.GetShuffledCards().ToList();

            var cardsToDeal = allCards.Take(allCards.Count - 1);
            var firstCard = allCards.Last();

            // so we deal the cards to be dealt starting with the first player in our list until we run out of cards
            PlayerCardStacks = Players.ToDictionary(x => x, x => new CardSet($"{x.Name} hidden cards", CardSetStates.Blind));
            PlayerHandsWon = Players.ToDictionary(x => x, _ => 0);
            var dealtCardNumber = 0;
            foreach (var cardToDeal in cardsToDeal)
            {
                var playerToDealTo = Players[dealtCardNumber % Players.Count];
                PlayerCardStacks[playerToDealTo].Add(cardToDeal);

                dealtCardNumber++;
            }

            // final card is dealt
            InPlayStack = new CardSet("In play stack", CardSetStates.OnTable);
            InPlayStack.Add(firstCard);

            // set the players turn
            CurrentPlayersTurn = Players.First();

            SendGameState(SnapActions.CardLaid, "Dealer");
        }

        /// <summary>
        /// Send the current game state to all users
        /// </summary>
        private void SendGameState(SnapActions lastAction, string byName)
        {
            // get the current state of the game
            var gameState = new SnapGameState
            {
                GameStateId = GameStatesList.Count,
                LastAction = lastAction,
                LastActionByName = byName,
                PlayerNameToCardsInHand = PlayerCardStacks.ToDictionary(x => x.Key.Name, x => x.Value.Count),
                PlayerNameToHandsWon = PlayerHandsWon.ToDictionary(x => x.Key.Name, x => x.Value),
                CardsOnStack = InPlayStack.Select(x => x.ToString()).ToArray(),
                PlayerIdTurn = CurrentPlayersTurn?.Id,
            };

            GameStatesList.Add(gameState);

            Console.WriteLine();
            Console.WriteLine(gameState);


            // we need to tell the user what actions they can perform
            var availableActionsDictionary = Players.ToDictionary(x => x.Id, _ => InPlayStack.Count > 1 
                ? new List<string> { nameof(CallSnapAction) } 
                : new List<string>());

            // the user who's turn it is can lay a card IF they have cards in there stack
            if (CurrentPlayersTurn != null)
            {
                if (PlayerCardStacks[CurrentPlayersTurn].Any())
                {
                    availableActionsDictionary[CurrentPlayersTurn.Id].Add(nameof(LayCardAction));
                }
                else
                {
                    availableActionsDictionary[CurrentPlayersTurn.Id].Add(nameof(SkipGoAction));
                }
            }

            Players.ForEach(x => x.ReceiveGameState(gameState, availableActionsDictionary[x.Id]));
        }

        public PlayActionResponse PlayAction(IPlayerAction action)
        {
            // todo add a lock around this to stop multiple actions being played

            if (action == null) throw new ArgumentNullException(nameof(action));

            var player = Players.Single(x => x.Id == action.PlayerId);

            if (action.LastGameStateId != GameStatesList.Last().GameStateId)
            {
                return new PlayActionResponse { 
                    Success = false, 
                    FailureReason = $"Action had last id of {action.LastGameStateId} but the actual last id is {GameStatesList.Last().GameStateId}, player ({player.Name}) is out of sync" 
                };
            }

            if (action is LayCardAction layCardAction)
            {
                if (action.PlayerId != CurrentPlayersTurn.Id)
                {
                    return new PlayActionResponse
                    {
                        Success = false,
                        FailureReason = $"Action came from user {action.PlayerId} but it is currently the turn of {CurrentPlayersTurn.Id}, player is out of sync"
                    };
                }

                // lay the card
                PlayerCardStacks[player].MoveFirstTo(InPlayStack);

                // switch to next player
                NextPlayersTurn();

                // update everyone
                SendGameState(SnapActions.CardLaid, player.Name);

                return new PlayActionResponse
                {
                    Success = true,
                };
            }

            if (action is SkipGoAction skipGoAction)
            {
                if (action.PlayerId != CurrentPlayersTurn.Id)
                {
                    return new PlayActionResponse
                    {
                        Success = false,
                        FailureReason = $"Action came from user {action.PlayerId} but it is currently the turn of {CurrentPlayersTurn.Id}, player is out of sync"
                    };
                }

                // verify that the user has no cards
                if (PlayerCardStacks[player].Any())
                {
                    return new PlayActionResponse
                    {
                        Success = false,
                        FailureReason = $"User tried to skip go but has {PlayerCardStacks[player].Count()} cards in there stack",
                    };
                }

                // switch to next player
                NextPlayersTurn();

                // update everyone
                SendGameState(SnapActions.SkippedGo, player.Name);

                return new PlayActionResponse
                {
                    Success = true,
                };
            }

            if (action is CallSnapAction callSnapAction)
            {
                // any user can do this action

                // grab the last two cards
                var lastTwoCards = InPlayStack.Skip(InPlayStack.Count - 2).Take(2).ToArray();

                if (lastTwoCards[0].Number == lastTwoCards[1].Number)
                {
                    // success
                    Console.WriteLine($"Snap called successfully by {player.Name}");

                    PlayerHandsWon[player]++;

                    Console.WriteLine($"{InPlayStack.Count} cards sent to {player.Name}");

                    // the deck (and next turn) goes to the winning player
                    InPlayStack.MoveAllTo(PlayerCardStacks[player]);
                    CurrentPlayersTurn = player;

                    // does the winning user have all the cards?
                    if (PlayerCardStacks[player].Count == Deck.Cards.Count)
                    {
                        GameWon(player);
                        return new PlayActionResponse
                        {
                            Success = true,
                        };
                    }

                    SendGameState(SnapActions.SnapSuccess, player.Name);
                }
                else
                {
                    // bad call
                    Console.WriteLine($"Snap called INCORRECTLY by {player.Name}");

                    var opponent = GetOpponent(player);
                    PlayerHandsWon[opponent]++;

                    Console.WriteLine($"{InPlayStack.Count} cards sent to {opponent.Name}");

                    // the deck (and next turn) goes to the opponent
                    InPlayStack.MoveAllTo(PlayerCardStacks[opponent]);
                    CurrentPlayersTurn = opponent;

                    // does the winning user have all the cards?
                    if (PlayerCardStacks[opponent].Count == Deck.Cards.Count)
                    {
                        GameWon(opponent);
                        return new PlayActionResponse
                        {
                            Success = true,
                        };
                    }

                    SendGameState(SnapActions.SnapFail, player.Name);
                }
                
                // lay the card
                
                PlayerCardStacks[player].MoveFirstTo(InPlayStack);

                // switch to next player
                NextPlayersTurn();

                // update everyone
                SendGameState(SnapActions.CardLaid, player.Name);

                return new PlayActionResponse
                {
                    Success = true,
                };
            }

            throw new NotImplementedException($"Unknown action type {action.GetType().Name}");
        }

        private void GameWon(SnapGamePlayer player)
        {
            Console.WriteLine($"{player.Name} has all the cards!");
            Console.WriteLine($"{player.Name} wins");

            CurrentPlayersTurn = null;
            Status = GameStatus.Finished;

            SendGameState(SnapActions.SnapSuccess, player.Name);
        }

        private void NextPlayersTurn()
        {
            CurrentPlayersTurn = GetOpponent(CurrentPlayersTurn);
        }

        private SnapGamePlayer GetOpponent(SnapGamePlayer player)
            => Players.Single(x => !x.Equals(player));
    }

    public class PlayActionResponse
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
    }

    /// <summary>
    /// A player plays a card from their own cardstack
    /// </summary>
    /// <param name="playerId"></param>
    public class LayCardAction : BasePlayerAction
    { 
        public LayCardAction(Guid playerId, int lastGameStateId) : base(playerId, lastGameStateId)
        { }
    }

    /// <summary>
    /// A player skips a go as they have no cards in there card stack
    /// </summary>
    /// <param name="playerId"></param>
    public class SkipGoAction : BasePlayerAction
    {
        public SkipGoAction(Guid playerId, int lastGameStateId) : base(playerId, lastGameStateId)
        { }
    }

    /// <summary>
    /// A player calls snap
    /// </summary>
    /// <param name="playerId"></param>
    public class CallSnapAction : BasePlayerAction
    {
        public CallSnapAction(Guid playerId, int lastGameStateId) : base(playerId, lastGameStateId)
        { }
    }

    public abstract class BasePlayerAction : IPlayerAction
    {
        public Guid PlayerId { get; }
        public int LastGameStateId { get; }

        public BasePlayerAction(Guid playerId, int lastGameStateId)
        {
            PlayerId = playerId;
            LastGameStateId = lastGameStateId;
        }
    }


    public interface IPlayerAction
    {
        Guid PlayerId { get; }
        int LastGameStateId { get; }
    }

    /// <summary>
    /// A game state that can be shared with players at any point to inform the current situation, initially is public so sent to all, but will also be a private variant just sent to a specific user
    /// </summary>
    public class SnapGameState
    {
        public int GameStateId { get; set; }

        public Dictionary<string, int> PlayerNameToCardsInHand { get; set; }

        public Dictionary<string, int> PlayerNameToHandsWon { get; set; }

        public string[] CardsOnStack { get; set; }

        public SnapActions LastAction { get; set; }

        public string LastActionByName { get; set; }

        public Guid? PlayerIdTurn { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(GameStateId)}: {GameStateId}");
            stringBuilder.AppendLine($"{nameof(LastAction)}: {LastAction}");
            stringBuilder.AppendLine($"{nameof(LastActionByName)}: {LastActionByName}");
            stringBuilder.AppendLine($"{nameof(PlayerIdTurn)}: {PlayerIdTurn}");
            stringBuilder.AppendLine($"Hands won: " + string.Join(", ", PlayerNameToHandsWon.Select(x => $"{x.Key} ({x.Value})")));
            stringBuilder.AppendLine($"Cards in hand: " + string.Join(", ", PlayerNameToCardsInHand.Select(x => $"{x.Key} ({x.Value})")));
            stringBuilder.AppendLine($"Cards on stack: " + string.Join(", ", CardsOnStack));

            return stringBuilder.ToString();
        }
    }

    public enum SnapActions
    {
        CardLaid,
        SnapSuccess,
        SnapFail,
        SkippedGo,
    }

    public interface IGamePlayer
    {
        Task StartGamePlayingLoopAsync();
        void StopGamePlayingLoop();
    }

    /// <summary>
    /// This is effectively happenning on the client, this will eventually be split in two when comms set up
    /// </summary>
    public class SnapGamePlayer : IGamePlayer
    {
        public string Name => Player.Name;
        public Guid Id { get; }

        private readonly SnapGame Game;
        private readonly Player Player;

        private SnapGameState LatestGameState;
        private List<string> AvailableActions;

        public SnapGamePlayer(SnapGame game, Player player)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            if (player == null) throw new ArgumentNullException(nameof(player));

            Game = game;
            Player = player;
            Id = Guid.NewGuid();
        }

        private bool _continueGamePlayingLoop;

        public async Task StartGamePlayingLoopAsync()
        {
            _continueGamePlayingLoop = true;
            var lastSeenGameStateId = -1;

            while (_continueGamePlayingLoop && Game.Status != GameStatus.Finished)
            {
                if (lastSeenGameStateId == (LatestGameState?.GameStateId ?? -1))
                {
                    // so we've seen this before, we didn't do any actions before, so just go back to sleep for longer
                    Thread.Sleep(200);
                    continue;
                }

                // so a new game state has been received
                lastSeenGameStateId = LatestGameState.GameStateId;

                if (AvailableActions.Count > 0)
                {
                    Think();
                }
            }
        }

        public void StopGamePlayingLoop()
        {
            _continueGamePlayingLoop = false;
        }

        public void ReceiveGameState(SnapGameState gameState, List<string> availableActions)
        {
            LatestGameState = gameState;
            AvailableActions = availableActions;
        }

        public void Think()
        {
            Thread.Sleep(100);

            // check the last two cards
            if (LatestGameState.CardsOnStack.Count() >= 2)
            {
                var lastTwoCardNumbers = LatestGameState.CardsOnStack.Skip(LatestGameState.CardsOnStack.Length - 2).Take(2).Select(x => x.Substring(x.Length - 1)).ToList();

                if (lastTwoCardNumbers[0] == lastTwoCardNumbers[1])
                {
                    // we have a match! call snap
                    var action = new CallSnapAction(Id, LatestGameState.GameStateId);

                    var actionResponse = Game.PlayAction(action);

                    if (!actionResponse.Success)
                    {
                        throw new Exception($"Playing action failed! Reason = {actionResponse.FailureReason}");
                    }
                }
            }

            if (LatestGameState.PlayerIdTurn == Id)
            {
                // its my turn
                if (AvailableActions.Contains(nameof(LayCardAction)))
                {
                    var action = new LayCardAction(Id, LatestGameState.GameStateId);

                    var actionResponse = Game.PlayAction(action);

                    if (!actionResponse.Success)
                    {
                        throw new Exception($"Playing action failed! Reason = {actionResponse.FailureReason}");
                    }
                }
                else
                {
                    var action = new SkipGoAction(Id, LatestGameState.GameStateId);

                    var actionResponse = Game.PlayAction(action);

                    if (!actionResponse.Success)
                    {
                        throw new Exception($"Playing action failed! Reason = {actionResponse.FailureReason}");
                    }
                }
            }
        }
    }

    public enum GameStatus
    {
        NotInitialised,
        WaitingForPlayers,
        ReadyToStart,
        Playing,
        Finished,
    }

    public class JoinGameResponse
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public IGamePlayer GamePlayer { get; set; }
    }

    public interface IGame
    {
        public string Name { get; }
        public int MinPlayers { get; }
        public int MaxPlayers { get; }
        public GameStatus Status { get; }
    }

    public class SnapGameOptions
    {
        /// <summary>
        /// The number of card numbers to use, full deck == 13
        /// Bring it down to a smaller number to make the game simpler
        /// </summary>
        public int NumbersInUse { get; set; } = 13;

        public bool AutoStartWhenMinPlayersReached { get; set; } = true;
    }
}
