﻿using System;
using System.Threading.Tasks;
using PlanningPoker.Engine.Core;
using PlanningPoker.Hub.Client.Abstractions.ViewModels;
using PlanningPoker.Server.ViewModelMappers;

namespace PlanningPoker.Server.Hubs
{
    public class PlanningPokerHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IPlanningPokerEngine _pokerEngine;

        public PlanningPokerHub(IPlanningPokerEngine pokerEngine)
        {
            _pokerEngine = pokerEngine;
        }

        public async Task Connect(Guid id)
        {
            await Groups.AddToGroupAsync(GetPlayerPrivateId(), id.ToString());
        }

        public void Kick(Guid id, string initiatingPlayerPrivateId, int playerPublicIdToRemove)
        {
            _pokerEngine.Kick(id, initiatingPlayerPrivateId, playerPublicIdToRemove);
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _pokerEngine.SleepInAllRooms(GetPlayerPrivateId());
            await base.OnDisconnectedAsync(exception);
        }

        public ServerCreationResult Create(string desiredCardSet)
        {
            var (wasCreated, serverId, validationMessage) = _pokerEngine.CreateRoom(desiredCardSet);
            var creationResult = new ServerCreationResult
            {
                Created = wasCreated,
                ServerId = serverId,
                ValidationMessage = validationMessage
            };
            
            return creationResult;
        }

        public bool Exists(Guid roomId)
        {
            return _pokerEngine.RoomExists(roomId);
        }

        public PlayerViewModel Join(Guid id, Guid recoveryId, string playerName, Engine.Core.Models.PlayerType type, Engine.Core.Models.PlayerTag tag)
        {
            var joinedPlayer = _pokerEngine.JoinRoom(id, recoveryId, playerName, GetPlayerPrivateId(), type, tag);
            return joinedPlayer.Map(includePrivateId: true);
        }

        public void Vote(Guid serverId, string playerId, string vote)
        {
            _pokerEngine.Vote(serverId, playerId, vote);
        }

        public void UnVote(Guid serverId, string playerId)
        {
            _pokerEngine.RedactVote(serverId, playerId);
        }

        public void Clear(Guid serverId)
        {
            _pokerEngine.ClearVotes(serverId, GetPlayerPrivateId());
        }

        public void Show(Guid serverId)
        {
            _pokerEngine.ShowVotes(serverId, GetPlayerPrivateId());
        }

        public PlayerViewModel ChangePlayerType(Guid serverId, Engine.Core.Models.PlayerType newType)
        {
            var updatedPlayer = _pokerEngine.ChangePlayerType(serverId, GetPlayerPrivateId(), newType);
            return updatedPlayer.Map(includePrivateId: true);
        }

        private string GetPlayerPrivateId()
        {
            return Context.ConnectionId;
        }
    }
}