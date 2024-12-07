namespace PlanningPoker.Engine.Core.Models
{
    public class Vote
    {
        public PlayerTag Tag { get; set; }
        public string Value { get; set; }

        public Vote(PlayerTag tag, string value) 
        {
            Tag = tag;
            Value = value;
        }
    }
}
