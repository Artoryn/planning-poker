using System.Collections.Generic;

namespace PlanningPoker.Hub.Client.Abstractions.ViewModels
{
    public enum PlayerTag
    {
        None = 0,
        Developer = 100,
        Analyst = 101
    }

    public static class PlayerTagExtensions
    {
        private static Dictionary<int, int> tagMap = new Dictionary<int, int>()
        {
            { 0, 2 },
            { 100, 0 },
            { 200, 1 },
        };

        public static int SortOrder(this PlayerTag tag)
        {
            return tagMap.ContainsKey((int)tag) ? tagMap[(int)tag] : 99;
        }
    }
}
