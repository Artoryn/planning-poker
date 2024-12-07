namespace PlanningPoker.Hub.Client.Abstractions.ViewModels
{
    public class VoteViewModel
    {
        public PlayerTag Tag { get; set; }
        public string Value { get; set; }

        public VoteViewModel(PlayerTag tag, string value)
        {
            Tag = tag;
            Value = value;
        }

        public VoteViewModel(int tag, string value)
        {
            Tag = (PlayerTag)tag;
            Value = value;
        }

        public VoteViewModel(string value)
        {
            Tag = PlayerTag.None;
            Value = value;
        }
    }
}
