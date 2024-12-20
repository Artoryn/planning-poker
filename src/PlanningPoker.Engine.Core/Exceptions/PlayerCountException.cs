using System;

namespace PlanningPoker.Engine.Core.Exceptions
{
    internal class PlayerCountException : Exception
    {
        public PlayerCountException(int playerCount = 0) : base($"Max player count {(playerCount <= 0 ? "" : $"of {playerCount}")} was reached.")
        {

        }
    }
}
