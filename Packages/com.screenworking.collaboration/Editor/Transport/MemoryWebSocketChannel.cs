using System;
using System.Collections.Generic;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Editor.Transport
{
    /// <summary>
    /// In-memory loopback and multi-client broker channel used for local offline sessions and unit testing.
    /// </summary>
    public class MemoryWebSocketChannel : IWebSocketClient
    {
        private static readonly Dictionary<string, List<MemoryWebSocketChannel>> RoomBrokers = new Dictionary<string, List<MemoryWebSocketChannel>>();

        public bool IsConnected { get; private set; }
        public string RoomId { get; private set; }

        public event Action<CollaborationOperation> OnMessageReceived;
        public event Action<bool> OnConnectionStatusChanged;

        public void Connect(string serverUrl, string roomId, string token)
        {
            RoomId = roomId ?? "default-room";
            IsConnected = true;

            lock (RoomBrokers)
            {
                if (!RoomBrokers.TryGetValue(RoomId, out var channelList))
                {
                    channelList = new List<MemoryWebSocketChannel>();
                    RoomBrokers[RoomId] = channelList;
                }
                if (!channelList.Contains(this))
                {
                    channelList.Add(this);
                }
            }

            OnConnectionStatusChanged?.Invoke(true);
        }

        public void Disconnect()
        {
            IsConnected = false;
            lock (RoomBrokers)
            {
                if (!string.IsNullOrEmpty(RoomId) && RoomBrokers.TryGetValue(RoomId, out var channelList))
                {
                    channelList.Remove(this);
                }
            }
            OnConnectionStatusChanged?.Invoke(false);
        }

        public void Send(CollaborationOperation op)
        {
            if (!IsConnected || string.IsNullOrEmpty(RoomId)) return;

            List<MemoryWebSocketChannel> targets;
            lock (RoomBrokers)
            {
                if (!RoomBrokers.TryGetValue(RoomId, out var channelList))
                {
                    return;
                }
                targets = new List<MemoryWebSocketChannel>(channelList);
            }

            foreach (var peer in targets)
            {
                if (peer != this && peer.IsConnected)
                {
                    peer.ReceiveInbound(op);
                }
            }
        }

        public void ReceiveInbound(CollaborationOperation op)
        {
            OnMessageReceived?.Invoke(op);
        }

        public static void ResetBrokers()
        {
            lock (RoomBrokers)
            {
                RoomBrokers.Clear();
            }
        }
    }
}
