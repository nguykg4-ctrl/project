using System;
using System.ComponentModel.DataAnnotations;

namespace ScreenWorking.Server.API.Data.Entities
{
    public class RoomEntity
    {
        [Key]
        public string RoomId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public long CurrentSequence { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; }
    }
}
