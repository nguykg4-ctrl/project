using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ScreenWorking.Server.API.Services;

namespace ScreenWorking.Server.API.WebSockets
{
    public class CollaborationWebSocketHandler
    {
        private readonly RoomManager roomManager;
        private readonly IServiceScopeFactory scopeFactory;

        public CollaborationWebSocketHandler(RoomManager roomManager, IServiceScopeFactory scopeFactory)
        {
            this.roomManager = roomManager;
            this.scopeFactory = scopeFactory;
        }

        public async Task HandleConnectionAsync(HttpContext context)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            string roomId = context.Request.Query["roomId"].FirstOrDefault() ?? "default-room";
            string clientId = Guid.NewGuid().ToString("N");

            await roomManager.AddClientAsync(roomId, clientId, webSocket);

            var buffer = new byte[1024 * 64];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text || result.MessageType == WebSocketMessageType.Binary)
                    {
                        byte[] payload = new byte[result.Count];
                        Array.Copy(buffer, 0, payload, 0, result.Count);

                        // Broadcast operation payload to peers in room & record history
                        await roomManager.BroadcastAsync(roomId, clientId, payload);
                    }
                }
            }
            finally
            {
                roomManager.RemoveClient(roomId, clientId);
            }
        }
    }
}
