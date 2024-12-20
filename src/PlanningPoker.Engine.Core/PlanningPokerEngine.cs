using System;
using System.Collections.Generic;
using System.Linq;
using PlanningPoker.Engine.Core.Exceptions;
using PlanningPoker.Engine.Core.Models;
using PlanningPoker.Engine.Core.Models.Events;

namespace PlanningPoker.Engine.Core
{
    public interface IPlanningPokerEngine
    {
        event EventHandler<PlayerKickedEventArgs> PlayerKicked;
        event EventHandler<RoomUpdatedEventArgs> RoomUpdated;
        event EventHandler<LogUpdatedEventArgs> LogUpdated;
        event EventHandler<RoomClearedEventArgs> RoomCleared;

        void Kick(Guid id, string initiatingPlayerPrivateId, int playerPublicIdToRemove);
        void SleepInAllRooms(string playerPrivateId);
        (bool wasCreated, Guid? serverId, string? validationMessages) CreateRoom(string desiredCardSet);
        bool RoomExists(Guid roomId);
        Player JoinRoom(Guid id, Guid recoveryId, string playerName, string playerPrivateId, PlayerType type, PlayerTag tag);
        void Vote(Guid serverId, string playerPrivateId, string vote);
        void RedactVote(Guid serverId, string playerPrivateId);
        void ClearVotes(Guid serverId, string playerPrivateId);
        void ShowVotes(Guid serverId, string playerPrivateId);
        Player ChangePlayerType(Guid serverId, string playerPrivateId, PlayerType newType);
    }

    public class PlanningPokerEngine : IPlanningPokerEngine
    {
        private readonly IServerStore _serverStore;
        private const int MaxPlayerNameLength = 20;

        private static IList<string> FibCards => new List<string>
        {
            "1",
            "2",
            "3",
            "5",
            "8",
            "13",
            "21",
            "?",
            "de"
        };
        private static IList<string> HoursCards => new List<string>
        {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "?",
            "de"
        };

        public PlanningPokerEngine(
            IServerStore serverStore)
        {
            _serverStore = serverStore;

            _serverStore.Create(FibCards, new Guid("db164431-bf32-499a-8080-38eb5bb07768"), true); // Hitch
            _serverStore.Create(HoursCards, new Guid("db164431-bf32-499a-8080-38eb5bb07769"), true); // Hitch Hour Cards version
            _serverStore.Create(HoursCards, new Guid("ebe5200e-614b-4dea-a454-f814acf92f19"), true); // Inventory
            _serverStore.Create(FibCards, new Guid("ebe5200e-614b-4dea-a454-f814acf92f10"), true); // Inventory Fib Cards version
            _serverStore.Create(FibCards, new Guid("64abca5e-b4f0-4e3f-bdd7-ba36f1d485ef"), true); // Store
            _serverStore.Create(HoursCards, new Guid("64abca5e-b4f0-4e3f-bdd7-ba36f1d485ea"), true); // Store Hour Cards version
        }

        public event EventHandler<PlayerKickedEventArgs> PlayerKicked;
        public event EventHandler<RoomUpdatedEventArgs> RoomUpdated;
        public event EventHandler<LogUpdatedEventArgs> LogUpdated;
        public event EventHandler<RoomClearedEventArgs> RoomCleared;
        
        public void Kick(Guid id, string initiatingPlayerPrivateId, int playerPublicIdToRemove)
        {
            var server = _serverStore.Get(id);
            var player = server.GetPlayer(initiatingPlayerPrivateId);
            var wasRemoved = server.TryRemovePlayer(playerPublicIdToRemove, out var kickedPlayer);
            if (wasRemoved && kickedPlayer != null)
            {
                RaisePlayerKicked(id, kickedPlayer);
                RaiseRoomUpdated(id, server);
                RaiseLogUpdated(id, player.Name, $"Logged out {kickedPlayer.Name}.");
            }
        }

        public void SleepInAllRooms(string playerPrivateId)
        {
            var serversWithPlayer = SetPlayerToSleepOnAllServers(_serverStore.All(), playerPrivateId);
            foreach (var server in serversWithPlayer)
            {
                RaiseRoomUpdated(server.Id, server);
            }
        }

        public (bool wasCreated, Guid? serverId, string? validationMessages) CreateRoom(string desiredCardSet)
        {
            var isParsed = CardSetProcessor.TryParseCardSet(desiredCardSet, out var cardSet, out var validationMessage);
            if (!isParsed)
            {
                return (false, null, validationMessage);
            }

            if (_serverStore.Count() > 20)
            {
                return (false, null, "To many servers created. Please try an existing server or wait for auto cleanup.");
            }

            var server = _serverStore.Create(cardSet);
            return (true, server.Id, validationMessage);
        }

        public bool RoomExists(Guid roomId)
        {
            return _serverStore.Exists(roomId);
        }

        public Player JoinRoom(Guid id, Guid recoveryId, string playerName, string playerPrivateId, PlayerType type, PlayerTag tag)
        {
            if (string.IsNullOrWhiteSpace(playerName)) throw new MissingPlayerNameException();

            var server = _serverStore.Get(id);
            
            var formattedPlayerName = playerName.Length > MaxPlayerNameLength ? playerName.Substring(0, MaxPlayerNameLength) : playerName;
            var newPlayer = server.AddOrUpdatePlayer(recoveryId, playerPrivateId, formattedPlayerName, type, tag);

            ClearOldAsleepPlayers(server, playerName);

            RaiseRoomUpdated(id, server);
            RaiseLogUpdated(id, newPlayer.Name, "Joined the server.");
            return newPlayer;
        }

        private void ClearOldAsleepPlayers(PokerServer server, string playerName)
        {
            var asleepPlayers = server.Players.Where(x => x.Value.Mode == PlayerMode.Asleep);

            var player = asleepPlayers.Select(x => x.Value).FirstOrDefault(x => x.Name == playerName);
            if (player != null)
            {
                server.RemovePlayer(player.Id);
            }
        }

        public void Vote(Guid serverId, string playerPrivateId, string vote)
        {
            var server = _serverStore.Get(serverId);
            if (!server.CurrentSession.CardSet.Contains(vote)) throw new VoteException($"Vote does not exist in card set.");
            if (!server.CurrentSession.CanVote) throw new VoteException($"Session not in state where players can vote.");
            if (!server.Players.ContainsKey(playerPrivateId)) throw new VoteException($"Player is not part of session.");

            var player = server.GetPlayer(playerPrivateId);
            if (player.Type == PlayerType.Observer) throw new VoteException($"Player is of type '{player.Type}' and cannot vote.");
            if (player.Mode == PlayerMode.Asleep) throw new VoteException($"Player is in mode '{player.Mode}', and cannot vote.");

            server.CurrentSession.SetVote(player.PublicId, new Models.Vote(player.Tag, vote));
            RaiseRoomUpdated(server.Id, server);
            RaiseLogUpdated(server.Id, player.Name, "Voted.");
        }

        public void RedactVote(Guid serverId, string playerPrivateId)
        {
            var server = _serverStore.Get(serverId);
            if (!server.CurrentSession.CanVote) throw new VoteException($"Session not in state where players can unvote.");
            if (!server.Players.ContainsKey(playerPrivateId)) throw new VoteException($"Player is not part of session.");

            var player = server.GetPlayer(playerPrivateId);
            if (player.Type == PlayerType.Observer) throw new VoteException($"Player is of type '{player.Type}' and cannot vote.");
            if (player.Mode == PlayerMode.Asleep) throw new VoteException($"Player is in mode '{player.Mode}', and cannot vote.");

            server.CurrentSession.RemoveVote(player.PublicId);
            RaiseRoomUpdated(server.Id, server);
            RaiseLogUpdated(server.Id, player.Name, "Redacted their vote.");
        }

        public void ClearVotes(Guid serverId, string playerPrivateId)
        {
            var server = _serverStore.Get(serverId);
            if (!server.CurrentSession.CanClear) throw new VoteException($"Session not in state where votes can be cleared.");

            server.CurrentSession.ClearVotes();
            var player = server.GetPlayer(playerPrivateId);
            RaiseRoomUpdated(server.Id, server);
            RaiseRoomCleared(server.Id);
            RaiseLogUpdated(server.Id, player.Name, "Cleared all votes.");
        }

        public void ShowVotes(Guid serverId, string playerPrivateId)
        {
            var server = _serverStore.Get(serverId);
            if (!server.CurrentSession.CanShow(server.Players)) throw new VoteException($"Session not in state where votes can be shown.");

            server.CurrentSession.ShowVotes();
            var player = server.GetPlayer(playerPrivateId); 
            RaiseRoomUpdated(server.Id, server);
            RaiseLogUpdated(server.Id, player.Name, "Made all votes visible.");
        }

        public Player ChangePlayerType(Guid serverId, string playerPrivateId, PlayerType newType)
        {
            var server = _serverStore.Get(serverId);
            var player = server.GetPlayer(playerPrivateId);
            if (server.CurrentSession.HasVoted(player.PublicId))
            {
                throw new ChangePlayerTypeException($"Cannot change from playertype '{PlayerType.Participant}', when player has voted.");
            }

            server.ChangePlayerType(player, newType);
            RaiseRoomUpdated(server.Id, server);
            RaiseLogUpdated(server.Id, player.Name, $"Changed their player type to {newType}.");
            return player;
        }

        private IList<PokerServer> SetPlayerToSleepOnAllServers(IEnumerable<PokerServer> servers, string playerPrivateId)
        {
            var serversWithUser = servers.Where(s => s.Players.ContainsKey(playerPrivateId)).ToList();
            foreach (var server in serversWithUser)
            {
                server.SleepPlayer(playerPrivateId);
            }

            return serversWithUser;
        }

        private void RaiseRoomCleared(Guid serverId)
        {
            RoomCleared.Invoke(this, new RoomClearedEventArgs(serverId));
        }

        private void RaiseLogUpdated(Guid serverId, string initiatingPlayerName, string logMessage)
        {
            LogUpdated.Invoke(this, new LogUpdatedEventArgs(serverId, initiatingPlayerName, logMessage));
        }

        private void RaiseRoomUpdated(Guid serverId, PokerServer updatedServer)
        {
            RoomUpdated.Invoke(this, new RoomUpdatedEventArgs(serverId, updatedServer));
        }

        private void RaisePlayerKicked(Guid serverId, Player kickedPlayer)
        {
            PlayerKicked.Invoke(this, new PlayerKickedEventArgs(serverId, kickedPlayer));
        }
    }
}
