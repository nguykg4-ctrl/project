using System;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Editor.Transport
{
    /// <summary>
    /// Contract for WebSocket transport implementations.
    /// </summary>
    public interface IWebSocketClient
    {
        bool IsConnected { get; }
        event Action<CollaborationOperation> OnMessageReceived;
        event Action<bool> OnConnectionStatusChanged;

        void Connect(string serverUrl, string roomId, string token);
        void Disconnect();
        void Send(CollaborationOperation op);
    }
}
