﻿using System.Collections.Generic;

namespace PlanningPoker.Hub.Client.Abstractions.ViewModels.Poker
{
    public class PokerSessionViewModel
    {
        public bool IsShown { get; set; }

        public bool CanShow { get; set; }

        public bool CanClear { get; set; }

        public bool CanVote { get; set; }

        public IDictionary<string, VoteViewModel>? Votes { get; set; }

        public IList<string>? CardSet { get; set; }
    }
}