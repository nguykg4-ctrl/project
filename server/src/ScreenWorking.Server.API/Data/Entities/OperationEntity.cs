using System;
using System.ComponentModel.DataAnnotations;

namespace ScreenWorking.Server.API.Data.Entities
{
    public class OperationEntity
    {
        [Key]
        public long Id { get; set; }
        public string RoomId { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public long ServerSequence { get; set; }
        public long LamportTimestamp { get; set; }
        public int OpType { get; set; }
        public string TargetObjectId { get; set; } = string.Empty;
        public byte[] Payload { get; set; } = Array.Empty<byte>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
