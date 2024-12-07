using System.Collections.Generic;
using System.Linq;
using PlanningPoker.Engine.Core.Models;
using PlanningPoker.Engine.Core.Models.Poker;
using PlanningPoker.Hub.Client.Abstractions.ViewModels.Poker;
using VoteViewModel = PlanningPoker.Hub.Client.Abstractions.ViewModels.VoteViewModel;

namespace PlanningPoker.Server.ViewModelMappers
{
    public static class PokerSessionViewModelMapper
    {
        public static PokerSessionViewModel Map(this PokerSession session, IDictionary<string, Player> participants)
        {
            var votes = MapVotes(session);
            var viewModel = new PokerSessionViewModel
            {
                Votes = votes,
                IsShown = session.IsShown,
                CanClear = session.CanClear,
                CanShow = session.CanShow(participants),
                CanVote = session.CanVote,
                CardSet = session.CardSet
            };

            return viewModel;
        }

        private static IDictionary<string, VoteViewModel> MapVotes(PokerSession session)
        {
            var votes = session.IsShown
                ? session.Votes.ToDictionary(pair => pair.Key.ToString(), pair => new VoteViewModel((int)pair.Value.Tag, pair.Value.Value))
                : session.Votes.ToDictionary(pair => pair.Key.ToString(), pair => new VoteViewModel("?"));

            return votes;
        }
    }
}