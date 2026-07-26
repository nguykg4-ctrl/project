using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScreenWorking.Collaboration.Editor.Models;
using ScreenWorking.Collaboration.Shared.Protocol;
using UnityEngine;

namespace ScreenWorking.Collaboration.Editor.Transport
{
    /// <summary>
    /// Real WebSocket network client implementation using System.Net.WebSockets.ClientWebSocket.
    /// Manages network frame transmission and background receive thread.
    /// </summary>
    public class RealWebSocketClient : IWebSocketClient
    {
        private ClientWebSocket socket;
        private CancellationTokenSource cts;

        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;

        public event Action<CollaborationOperation> OnMessageReceived;
        public event Action<bool> OnConnectionStatusChanged;

        public async void Connect(string serverUrl, string roomId, string token)
        {
            if (IsConnected) return;

            try
            {
                socket = new ClientWebSocket();
                cts = new CancellationTokenSource();

                string fullUri = serverUrl;
                if (!fullUri.Contains("?"))
                {
                    fullUri += $"?roomId={Uri.EscapeDataString(roomId)}";
                }
                else
                {
                    fullUri += $"&roomId={Uri.EscapeDataString(roomId)}";
                }

                await socket.ConnectAsync(new Uri(fullUri), cts.Token);
                OnConnectionStatusChanged?.Invoke(true);

                _ = Task.Run(() => ReceiveLoop(cts.Token));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[screen working] WebSocket Connection Error: {ex.Message}");
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        public async void Disconnect()
        {
            if (socket == null) return;

            try
            {
                cts?.Cancel();
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
                }
            }
            catch { }
            finally
            {
                socket?.Dispose();
                socket = null;
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        public async void Send(CollaborationOperation op)
        {
            if (!IsConnected || op == null) return;

            try
            {
                byte[] bytes = MessagePackSerializerWrapper.Serialize(op);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[screen working] WebSocket Send Error: {ex.Message}");
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[1024 * 64];
            while (!token.IsCancellationRequested && socket != null && socket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    if (result.Count > 0)
                    {
                        byte[] payload = new byte[result.Count];
                        Array.Copy(buffer, 0, payload, 0, result.Count);

                        var op = MessagePackSerializerWrapper.Deserialize(payload);
                        if (op != null)
                        {
                            OnMessageReceived?.Invoke(op);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[screen working] WebSocket Receive Error: {ex.Message}");
                    break;
                }
            }

            OnConnectionStatusChanged?.Invoke(false);
        }
    }
}
