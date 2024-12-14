using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PlanningPoker.Engine.Core.Models.Poker
{
    public class PokerSession
    {
        public PokerSession(IList<string> cardSet)
        {
            IsShown = false;
            Votes = new ConcurrentDictionary<int, Vote>();
            CardSet = cardSet;
        }

        public bool IsShown { get; set; }

        public bool CanShow(IDictionary<string, Player> participants)
        {
            return 
                Votes.Count != 0
                && !IsShown;
        }

        public bool CanClear => Votes.Any();

        public bool CanVote => !IsShown;

        public IDictionary<int, Vote> Votes { get; set; }

        public IList<string> CardSet { get; set; }

        public void SetVote(int playerPublicId, Vote vote)
        {
            Votes[playerPublicId] = vote;
        }

        public void RemoveVote(int playerPublicId)
        {
            Votes.Remove(playerPublicId);
        }

        public void ClearVotes()
        {
            Votes.Clear();
            IsShown = false;
        }

        public void ShowVotes()
        {
            IsShown = true;
        }

        public void RemovePlayer(int playerPublicId)
        {
            Votes.Remove(playerPublicId);
            if (!Votes.Any())
            {
                IsShown = false;
            }
        }

        public bool HasVoted(int playerPublicId)
        {
            return Votes.ContainsKey(playerPublicId);
        }
    }
}