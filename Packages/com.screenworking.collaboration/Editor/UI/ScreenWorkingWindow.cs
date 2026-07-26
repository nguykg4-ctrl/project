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
    /// Manages session configuration, role selection (Host vs Team Member), automatic EditorPrefs saving, and active peer tracking.
    /// </summary>
    public class ScreenWorkingWindow : EditorWindow
    {
        private const string PREF_SERVER_URL = "ScreenWorking_ServerUrl";
        private const string PREF_ROOM_CODE = "ScreenWorking_RoomCode";
        private const string PREF_USERNAME = "ScreenWorking_Username";
        private const string PREF_ROLE = "ScreenWorking_UserRole";

        private TextField serverUrlField;
        private TextField roomIdField;
        private TextField usernameField;
        private EnumField roleField;

        private Button createHostButton;
        private Button joinTeamButton;
        private Button disconnectButton;
        private Label statusLabel;
        private ListView userListView;

        private readonly List<string> activeUserNames = new List<string>();
        private CollaborationClientEngine clientEngine;

        public enum UserRole
        {
            Host,
            TeamMember
        }

        [MenuItem("Window/Collaboration/[screen working]", false, 2000)]
        [MenuItem("Window/[screen working]", false, 2001)]
        [MenuItem("Window/ScreenWorking Collaboration", false, 2002)]
        public static void OpenWindow()
        {
            var window = GetWindow<ScreenWorkingWindow>();
            window.titleContent = new GUIContent("[screen working]");
            window.minSize = new Vector2(420, 560);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            // Title Header
            var titleLabel = new Label("[screen working] Real-Time Scene Collaboration");
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 12;
            root.Add(titleLabel);

            // Server & Session Settings Group Box
            var connBox = new GroupBox();
            connBox.text = "Saved Session & Team Settings";

            string savedUrl = EditorPrefs.GetString(PREF_SERVER_URL, "wss://project-1-31b9.onrender.com/ws");
            string savedRoom = EditorPrefs.GetString(PREF_ROOM_CODE, "team-room-1");
            string savedUser = EditorPrefs.GetString(PREF_USERNAME, Environment.UserName);
            UserRole savedRole = (UserRole)EditorPrefs.GetInt(PREF_ROLE, (int)UserRole.Host);

            serverUrlField = new TextField("Server URL") { value = savedUrl };
            serverUrlField.RegisterValueChangedCallback(evt => EditorPrefs.SetString(PREF_SERVER_URL, evt.newValue));

            roomIdField = new TextField("Room Code") { value = savedRoom };
            roomIdField.RegisterValueChangedCallback(evt => EditorPrefs.SetString(PREF_ROOM_CODE, evt.newValue));

            usernameField = new TextField("Display Name") { value = savedUser };
            usernameField.RegisterValueChangedCallback(evt => EditorPrefs.SetString(PREF_USERNAME, evt.newValue));

            roleField = new EnumField("My Role", savedRole);
            roleField.RegisterValueChangedCallback(evt => EditorPrefs.SetInt(PREF_ROLE, Convert.ToInt32(evt.newValue)));

            connBox.Add(serverUrlField);
            connBox.Add(roomIdField);
            connBox.Add(usernameField);
            connBox.Add(roleField);
            root.Add(connBox);

            // Action Buttons
            var btnContainer = new VisualElement();
            btnContainer.style.flexDirection = FlexDirection.Row;
            btnContainer.style.marginTop = 10;

            createHostButton = new Button(() => ConnectSession(UserRole.Host)) { text = "👑 Create Room (Host)" };
            createHostButton.style.flexGrow = 1;
            createHostButton.style.height = 30;

            joinTeamButton = new Button(() => ConnectSession(UserRole.TeamMember)) { text = "🟢 Join Room (Team)" };
            joinTeamButton.style.flexGrow = 1;
            joinTeamButton.style.height = 30;

            btnContainer.Add(createHostButton);
            btnContainer.Add(joinTeamButton);
            root.Add(btnContainer);

            disconnectButton = new Button(OnDisconnectClicked) { text = "Leave Room" };
            disconnectButton.style.marginTop = 6;
            disconnectButton.style.height = 26;
            disconnectButton.SetEnabled(false);
            root.Add(disconnectButton);

            // Connection Status Indicator
            statusLabel = new Label("Status: Disconnected");
            statusLabel.style.marginTop = 10;
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            root.Add(statusLabel);

            // Connected Collaborators Group Box
            var usersBox = new GroupBox();
            usersBox.text = "Connected Team Members";
            usersBox.style.marginTop = 15;

            userListView = new ListView();
            userListView.style.height = 180;
            userListView.makeItem = () =>
            {
                var label = new Label();
                label.style.fontSize = 12;
                label.style.paddingLeft = 6;
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

        private void ConnectSession(UserRole role)
        {
            string url = serverUrlField.value;
            string room = roomIdField.value;
            string user = usernameField.value;

            // Persist parameters
            EditorPrefs.SetString(PREF_SERVER_URL, url);
            EditorPrefs.SetString(PREF_ROOM_CODE, room);
            EditorPrefs.SetString(PREF_USERNAME, user);
            EditorPrefs.SetInt(PREF_ROLE, (int)role);

            if (clientEngine == null)
            {
                clientEngine = new CollaborationClientEngine(user);
                clientEngine.OnRemoteOperationApplied += OnRemoteOpReceived;
            }

            clientEngine.Connect(url, room, "token");

            string roleTitle = role == UserRole.Host ? "Host" : "Team Member";
            statusLabel.text = $"Status: Connected to {room} ({roleTitle})";
            createHostButton.SetEnabled(false);
            joinTeamButton.SetEnabled(false);
            disconnectButton.SetEnabled(true);

            // Add local user to user list
            activeUserNames.Clear();
            string userBadge = role == UserRole.Host ? $"👑 {user} (You) - Host / Creator" : $"🟢 {user} (You) - Team Member";
            activeUserNames.Add(userBadge);
            userListView.Rebuild();

            // Broadcast presence to room peers
            string presenceData = $"{user}|{role}";
            var presenceOp = clientEngine.CRDTEngine.CreateLocalOperation(OperationType.PresenceUpdate, user, SerializedValue.FromString(presenceData));
            clientEngine.SendOperation(presenceOp);
        }

        private void OnDisconnectClicked()
        {
            if (clientEngine != null)
            {
                clientEngine.Disconnect();
            }

            statusLabel.text = "Status: Disconnected";
            createHostButton.SetEnabled(true);
            joinTeamButton.SetEnabled(true);
            disconnectButton.SetEnabled(false);

            activeUserNames.Clear();
            userListView.Rebuild();
        }

        private void OnRemoteOpReceived(CollaborationOperation op)
        {
            if (op == null) return;
            if (op.OpType == OperationType.PresenceUpdate && op.Payload != null)
            {
                string raw = op.Payload.StringValue;
                string peerName = raw;
                string badge = "🔵";
                string roleTag = "Remote Editor";

                if (raw.Contains("|"))
                {
                    var parts = raw.Split('|');
                    peerName = parts[0];
                    if (parts.Length > 1 && parts[1] == "Host")
                    {
                        badge = "👑";
                        roleTag = "Host / Creator";
                    }
                }

                string entry = $"{badge} {peerName} - {roleTag}";
                if (!activeUserNames.Contains(entry))
                {
                    activeUserNames.Add(entry);
                    userListView.Rebuild();
                }
            }
        }
    }
}
