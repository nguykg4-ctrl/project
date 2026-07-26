using System;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Editor.Engine
{
    /// <summary>
    /// Contract for the ScreenWorking client collaboration engine.
    /// </summary>
    public interface ICollaborationEngine
    {
        string ActorId { get; }
        string CurrentRoomId { get; }
        bool IsConnected { get; }

        event Action<CollaborationOperation> OnRemoteOperationApplied;
        event Action<bool> OnConnectionStateChanged;

        void Connect(string serverUrl, string roomId, string token);
        void Disconnect();
        void SendOperation(CollaborationOperation op);
        void ApplyRemoteOperation(CollaborationOperation op);
    }
}
