using System;
using System.Collections.Generic;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Shared.Protocol
{
    public enum MessageType
    {
        HandshakeRequest = 1,
        HandshakeResponse = 2,
        OperationBatch = 3,
        SnapshotRequest = 4,
        SnapshotResponse = 5,
        Heartbeat = 6,
        Error = 7
    }

    [Serializable]
    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string SessionToken { get; set; }
        public string RoomId { get; set; }
        public string SenderActorId { get; set; }
        public List<CollaborationOperation> Operations { get; set; } = new List<CollaborationOperation>();
        public string ErrorMessage { get; set; }
        public long ServerSequence { get; set; }
    }
}
