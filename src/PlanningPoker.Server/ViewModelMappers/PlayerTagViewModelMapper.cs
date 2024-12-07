using PlanningPoker.Hub.Client.Abstractions.ViewModels;
using System;

namespace PlanningPoker.Server.ViewModelMappers
{
    public static class PlayerTagViewModelMapper
    {
        public static PlayerTag Map(this Engine.Core.Models.PlayerTag playerType)
        {
            return playerType switch
            {
                Engine.Core.Models.PlayerTag.None => PlayerTag.None,
                Engine.Core.Models.PlayerTag.Developer => PlayerTag.Developer,
                Engine.Core.Models.PlayerTag.Analyst => PlayerTag.Analyst,
                _ => throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null)
            };
        }
    }
}
