using System;
using System.Collections.Generic;
using ScreenWorking.Collaboration.Editor.Engine;
using ScreenWorking.Collaboration.Editor.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenWorking.Collaboration.Editor.UI
{
    /// <summary>
    /// Main Editor UI Toolkit window for ScreenWorking collaboration.
    /// Provides connection management, room browser, active user list, lock inspector, and session diagnostics.
    /// </summary>
    public class ScreenWorkingWindow : EditorWindow
    {
        private TextField serverUrlField;
        private TextField roomIdField;
        private TextField usernameField;
        private Button connectButton;
        private Button disconnectButton;
        private Label statusLabel;
        private ListView userListView;

        private readonly List<string> activeUserNames = new List<string>();
        private CollaborationClientEngine clientEngine;

        [MenuItem("Window/Collaboration/[screen working]", false, 2000)]
        [MenuItem("Window/[screen working]", false, 2001)]
        [MenuItem("Window/ScreenWorking Collaboration", false, 2002)]
        public static void OpenWindow()
        {
            var window = GetWindow<ScreenWorkingWindow>();
            window.titleContent = new GUIContent("[screen working]");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            // Title Banner
            var titleLabel = new Label("[screen working] Real-Time Scene Collaboration");
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 10;
            root.Add(titleLabel);

            // Server Connection Group
            var connBox = new GroupBox();
            connBox.text = "Server & Session Settings";

            serverUrlField = new TextField("Server URL") { value = "ws://localhost:5000/ws" };
            roomIdField = new TextField("Room Code") { value = "team-room-1" };
            usernameField = new TextField("Display Name") { value = Environment.UserName };

            connBox.Add(serverUrlField);
            connBox.Add(roomIdField);
            connBox.Add(usernameField);
            root.Add(connBox);

            // Action Buttons
            var btnContainer = new VisualElement();
            btnContainer.style.flexDirection = FlexDirection.Row;
            btnContainer.style.marginTop = 10;

            connectButton = new Button(OnConnectClicked) { text = "Join Room" };
            connectButton.style.flexGrow = 1;

            disconnectButton = new Button(OnDisconnectClicked) { text = "Leave Room" };
            disconnectButton.style.flexGrow = 1;
            disconnectButton.SetEnabled(false);

            btnContainer.Add(connectButton);
            btnContainer.Add(disconnectButton);
            root.Add(btnContainer);

            // Connection Status Indicator
            statusLabel = new Label("Status: Disconnected");
            statusLabel.style.marginTop = 10;
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            root.Add(statusLabel);

            // Connected Users Section
            var usersBox = new GroupBox();
            usersBox.text = "Connected Collaborators";
            usersBox.style.marginTop = 15;

            userListView = new ListView();
            userListView.style.height = 150;
            userListView.makeItem = () =>
            {
                var label = new Label();
                label.style.fontSize = 12;
                label.style.paddingLeft = 4;
                label.style.paddingTop = 4;
                return label;
            };
            userListView.bindItem = (element, i) =>
            {
                if (i >= 0 && i < activeUserNames.Count)
                {
                    (element as Label).text = activeUserNames[i];
                }
            };
            userListView.itemsSource = activeUserNames;

            usersBox.Add(userListView);
            root.Add(usersBox);
        }

        private void OnConnectClicked()
        {
            string url = serverUrlField.value;
            string room = roomIdField.value;
            string user = usernameField.value;

            if (clientEngine == null)
            {
                clientEngine = new CollaborationClientEngine(user);
                clientEngine.OnRemoteOperationApplied += OnRemoteOpReceived;
            }

            clientEngine.Connect(url, room, "token");

            statusLabel.text = $"Status: Connected to {room}";
            connectButton.SetEnabled(false);
            disconnectButton.SetEnabled(true);

            // Add local user to user list
            activeUserNames.Clear();
            activeUserNames.Add($"🟢 {user} (You) - Active Editor");
            userListView.Rebuild();

            // Broadcast presence to room peers
            var presenceOp = clientEngine.CRDTEngine.CreateLocalOperation(OperationType.PresenceUpdate, user, SerializedValue.FromString(user));
            clientEngine.SendOperation(presenceOp);
        }

        private void OnDisconnectClicked()
        {
            if (clientEngine != null)
            {
                clientEngine.Disconnect();
            }

            statusLabel.text = "Status: Disconnected";
            connectButton.SetEnabled(true);
            disconnectButton.SetEnabled(false);

            activeUserNames.Clear();
            userListView.Rebuild();
        }

        private void OnRemoteOpReceived(CollaborationOperation op)
        {
            if (op == null) return;
            if (op.OpType == OperationType.PresenceUpdate && op.Payload != null)
            {
                string peerName = $"🔵 {op.Payload.StringValue} - Remote Editor";
                if (!activeUserNames.Contains(peerName))
                {
                    activeUserNames.Add(peerName);
                    userListView.Rebuild();
                }
            }
        }
    }
}
