using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PlanningPoker.Engine.Core;
using PlanningPoker.Engine.Core.Models;

namespace PlanningPoker.Server.Infrastructure
{
    public class ServerStore : IServerStore
    {
        private readonly IDictionary<Guid, PokerServer> _servers;

        public ServerStore()
        {
            _servers = new ConcurrentDictionary<Guid, PokerServer>();
        }

        public PokerServer Create(IList<string> cardSet, Guid? guid = null, bool persistent = false)
        {
            var newServerId = guid.HasValue ? guid.Value : Guid.NewGuid();
            var newServer = new PokerServer(newServerId, cardSet, persistent);
            _servers.Add(newServerId, newServer);
            return newServer;
        }

        public bool Exists(Guid id)
        {
            return _servers.ContainsKey(id);
        }

        public PokerServer Get(Guid id)
        {
            return _servers[id];
        }
        
        public ICollection<PokerServer> All()
        {
            return _servers.Values;
        }

        public int Count()
        {
            return _servers.Count;
        }

        public void Remove(PokerServer server)
        {
            _servers.Remove(server.Id);
        }
    }
}