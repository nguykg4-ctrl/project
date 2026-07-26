using System;
using System.ComponentModel.DataAnnotations;

namespace ScreenWorking.Server.API.Data.Entities
{
    public class SnapshotEntity
    {
        [Key]
        public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N");
        public string RoomId { get; set; } = string.Empty;
        public long SequenceNumber { get; set; }
        public byte[] Payload { get; set; } = Array.Empty<byte>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
