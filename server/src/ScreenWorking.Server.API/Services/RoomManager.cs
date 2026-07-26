using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ScreenWorking.Server.API.Services
{
    public class RoomSession
    {
        public string RoomId { get; set; } = string.Empty;
        public long CurrentSequence { get; set; }
        public ConcurrentDictionary<string, WebSocket> Clients { get; } = new ConcurrentDictionary<string, WebSocket>();
    }

    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, RoomSession> rooms = new ConcurrentDictionary<string, RoomSession>();

        public RoomSession GetOrCreateRoom(string roomId)
        {
            return rooms.GetOrAdd(roomId, id => new RoomSession { RoomId = id });
        }

        public void AddClient(string roomId, string clientId, WebSocket socket)
        {
            var session = GetOrCreateRoom(roomId);
            session.Clients[clientId] = socket;
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
