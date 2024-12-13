using Microsoft.AspNetCore.SignalR;
using PlanningPoker.Engine.Core;
using PlanningPoker.Engine.Core.Models.Events;
using PlanningPoker.Hub.Client.Abstractions;
using PlanningPoker.Hub.Client.Abstractions.ViewModels;
using PlanningPoker.Server.Infrastructure;
using PlanningPoker.Server.ViewModelMappers;
using System.Diagnostics;

namespace PlanningPoker.Server.Hubs
{
    public interface IPlanningPokerEventBroadcaster
    {
    }
    
    public class PlanningPokerEventBroadcaster : IPlanningPokerEventBroadcaster
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHubContext<PlanningPokerHub> _hubContext;

        public PlanningPokerEventBroadcaster(
            IHubContext<PlanningPokerHub> hubContext,
            IPlanningPokerEngine pokerEngine,
            IDateTimeProvider dateTimeProvider)
        {
            _hubContext = hubContext;
            _dateTimeProvider = dateTimeProvider;

            pokerEngine.LogUpdated += OnLogUpdated;
            pokerEngine.PlayerKicked += OnPlayerKicked;
            pokerEngine.RoomCleared += OnRoomCleared;
            pokerEngine.RoomUpdated += OnRoomUpdated;
        }

        private void OnRoomUpdated(object? sender, RoomUpdatedEventArgs e)
        {
            Debug.WriteLine(e.UpdatedServer?.Id);
            Debug.WriteLine(e.UpdatedServer?.CurrentSession?.Votes?.Count);
            var mappedValue = e.UpdatedServer?.Map() ?? null;
            Debug.WriteLine(mappedValue?.Id);
            Debug.WriteLine(mappedValue?.CurrentSession?.Votes?.Count);
            _hubContext.Clients.Group(e.ServerId.ToString()).SendAsync(BroadcastChannels.UPDATED, mappedValue);
        }

        private void OnRoomCleared(object? sender, RoomClearedEventArgs e)
        {
            _hubContext.Clients.Group(e.ServerId.ToString()).SendAsync(BroadcastChannels.CLEAR);
        }

        private void OnPlayerKicked(object? sender, PlayerKickedEventArgs e)
        {
            _hubContext.Clients.Group(e.ServerId.ToString()).SendAsync(BroadcastChannels.KICKED, e.KickedPlayer.Map(false));
        }

        private void OnLogUpdated(object? sender, LogUpdatedEventArgs e)
        {
            var now = _dateTimeProvider.GetUtcNow();
            var logMessage = new LogMessage
            {
                User = e.InitiatingPlayer,
                Message = e.LogMessage,
                Timestamp = now
            };

            _hubContext.Clients.Group(e.ServerId.ToString()).SendAsync(BroadcastChannels.LOG, logMessage);
        }
    }
}