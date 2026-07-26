using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenWorking.Server.API.Services
{
    public class RoomSession
    {
        public string RoomId { get; set; } = string.Empty;
        public long CurrentSequence { get; set; }
        public ConcurrentDictionary<string, WebSocket> Clients { get; } = new ConcurrentDictionary<string, WebSocket>();
        public List<byte[]> OperationHistory { get; } = new List<byte[]>();

        public void RecordOperation(byte[] payload)
        {
            lock (OperationHistory)
            {
                OperationHistory.Add(payload);
            }
        }

        public List<byte[]> GetHistoryCopy()
        {
            lock (OperationHistory)
            {
                return new List<byte[]>(OperationHistory);
            }
        }
    }

    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, RoomSession> rooms = new ConcurrentDictionary<string, RoomSession>();

        public RoomSession GetOrCreateRoom(string roomId)
        {
            return rooms.GetOrAdd(roomId, id => new RoomSession { RoomId = id });
        }

        public async Task AddClientAsync(string roomId, string clientId, WebSocket socket)
        {
            var session = GetOrCreateRoom(roomId);
            session.Clients[clientId] = socket;

            // Replay room operation history to newly joined client for instant catch-up
            var history = session.GetHistoryCopy();
            foreach (var payload in history)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public void RemoveClient(string roomId, string clientId)
        {
            if (rooms.TryGetValue(roomId, out var session))
            {
                session.Clients.TryRemove(clientId, out _);
            }
        }

        public async Task BroadcastAsync(string roomId, string senderClientId, byte[] payload)
        {
            if (!rooms.TryGetValue(roomId, out var session)) return;

            // Record operation in history for late joiners
            session.RecordOperation(payload);

            var segment = new ArraySegment<byte>(payload);
            foreach (var kvp in session.Clients)
            {
                if (kvp.Key != senderClientId && kvp.Value.State == WebSocketState.Open)
                {
                    await kvp.Value.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
