using System;

namespace PlanningPoker.Engine.Core.Models
{
    public class Player
    {
        public Player(
            string id, 
            Guid recoveryId,
            int publicId, 
            string name, 
            PlayerType type,
            PlayerTag tag = PlayerTag.None)
        {
            Id = id;
            RecoveryId = recoveryId;
            PublicId = publicId;
            Name = name;
            Type = type;
            Mode = PlayerMode.Awake;
            Tag = tag;
        }
        
        public string Id { get; set; }

        public Guid RecoveryId { get; set; }
        
        public int PublicId { get; set; }

        public string Name { get; set; }

        public PlayerType Type { get; set; }

        public PlayerMode Mode { get; set; }
        public DateTime? SleepDate { get; set; }

        public PlayerTag Tag { get; set; }
    }
}