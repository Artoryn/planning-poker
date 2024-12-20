﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlanningPoker.Engine.Core;
using PlanningPoker.Engine.Core.Models;

namespace PlanningPoker.Server.Infrastructure.HostedServices
{
    public class CleanupServerJob : IHostedService, IDisposable
    {
        private Timer? _timer;
        private static readonly TimeSpan RunFrequency = TimeSpan.FromMinutes(20);
        private readonly IServerStore _serverStore;

        public CleanupServerJob(IServerStore serverStore)
        {
            _serverStore = serverStore;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(_ => RunJob(cancellationToken), null, TimeSpan.Zero, RunFrequency);
            return Task.CompletedTask;
        }

        private void RunJob(CancellationToken cancellationToken)
        {
            var createdThreshold = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
            foreach (var server in _serverStore.All())
            {
                if (cancellationToken.IsCancellationRequested) break;

                var isEmptyOrAsleep = server.Players.All(p => p.Value.Mode != PlayerMode.Awake);
                if (server.Persistent)
                {
                    if (isEmptyOrAsleep)
                    {
                        var players = server.Players;
                        foreach (var p in players)
                        {
                            if (p.Value != null)
                                server.RemovePlayer(p.Value.Id);
                        }
                    }
                }
                else
                {
                    var isOld = createdThreshold > server.Created;
                    if (isOld && isEmptyOrAsleep)
                    {
                        _serverStore.Remove(server);
                    }
                }
            }

            var test = "test";
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }
        }
    }
}
