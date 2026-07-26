using System;

namespace ScreenWorking.Collaboration.Editor.Models
{
    [Serializable]
    public class CollaborationOperation
    {
        public ushort ProtocolVersion { get; set; } = 1;
        public string ProjectId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string OperationId { get; set; } = Guid.NewGuid().ToString("N");
        public string ActorId { get; set; } = string.Empty;
        public long ActorSequence { get; set; }
        public long ServerSequence { get; set; }
        public long LamportTimestamp { get; set; }
        public string TargetObjectId { get; set; } = string.Empty;
        public string TargetParentId { get; set; }
        public int SiblingIndex { get; set; } = -1;
        public string TargetComponentType { get; set; }
        public string PropertyPath { get; set; }
        public SerializedValue Payload { get; set; }
        public SerializedValue PreviousValue { get; set; }
        public string TransactionId { get; set; }
        public OperationType OpType { get; set; }
        public long BaseRevision { get; set; }
        public bool IsPersistent { get; set; } = true;
        public long TimestampTicks { get; set; } = DateTime.UtcNow.Ticks;

        public int CompareDeterministicTo(CollaborationOperation other)
        {
            if (other == null) return 1;
            if (LamportTimestamp != other.LamportTimestamp)
            {
                return LamportTimestamp.CompareTo(other.LamportTimestamp);
            }
            int actorComp = string.Compare(ActorId, other.ActorId, StringComparison.Ordinal);
            if (actorComp != 0)
            {
                return actorComp;
            }
            return ActorSequence.CompareTo(other.ActorSequence);
        }
    }
}
