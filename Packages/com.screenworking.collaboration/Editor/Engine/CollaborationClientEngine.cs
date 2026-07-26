using System;
using System.Collections.Generic;
using ScreenWorking.Collaboration.Editor.Capture;
using ScreenWorking.Collaboration.Editor.Identity;
using ScreenWorking.Collaboration.Editor.Models;
using ScreenWorking.Collaboration.Editor.Transport;
using UnityEditor;
using UnityEngine;

namespace ScreenWorking.Collaboration.Editor.Engine
{
    /// <summary>
    /// Core client collaboration engine managing network transport, change tracking, CRDT state evaluation, and main-thread application.
    /// </summary>
    public class CollaborationClientEngine : ICollaborationEngine
    {
        private readonly string actorId;
        private readonly LamportCRDTEngine crdtEngine;
        private readonly ScreenWorkingChangeTracker changeTracker;
        private readonly IWebSocketClient networkChannel;
        private readonly Queue<CollaborationOperation> inboundQueue = new Queue<CollaborationOperation>();

        public string ActorId => actorId;
        public string CurrentRoomId { get; private set; }
        public bool IsConnected => networkChannel != null && networkChannel.IsConnected;
        public LamportCRDTEngine CRDTEngine => crdtEngine;

        public event Action<CollaborationOperation> OnRemoteOperationApplied;
        public event Action<bool> OnConnectionStateChanged;

        public CollaborationClientEngine(string actorId, IWebSocketClient networkChannel = null)
        {
            this.actorId = actorId ?? Guid.NewGuid().ToString("N");
            this.crdtEngine = new LamportCRDTEngine(this.actorId);
            this.changeTracker = new ScreenWorkingChangeTracker();
            this.networkChannel = networkChannel ?? new RealWebSocketClient();

            this.changeTracker.OnOperationCaptured += OnLocalOperationCaptured;
            this.networkChannel.OnMessageReceived += OnNetworkMessageReceived;
        }

        public void Connect(string serverUrl, string roomId, string token)
        {
            CurrentRoomId = roomId;
            this.networkChannel.Connect(serverUrl, roomId, token);
            this.changeTracker.StartTracking();
            EditorApplication.update += ProcessInboundQueue;
            OnConnectionStateChanged?.Invoke(true);
        }

        public void Disconnect()
        {
            EditorApplication.update -= ProcessInboundQueue;
            this.changeTracker.StopTracking();
            this.networkChannel.Disconnect();
            OnConnectionStateChanged?.Invoke(false);
        }

        public void SendOperation(CollaborationOperation op)
        {
            if (op == null) return;
            op.ActorId = actorId;
            op.RoomId = CurrentRoomId;

            this.networkChannel.Send(op);
        }

        public void ApplyRemoteOperation(CollaborationOperation op)
        {
            if (op == null || op.ActorId == actorId) return;

            if (!crdtEngine.ProcessIncomingOperation(op))
            {
                return; // Discarded due to conflict resolution or idempotency
            }

            using (ScreenWorkingSyncScope.SuppressLocalCapture())
            {
                ExecuteOperationOnScene(op);
                SceneView.RepaintAll();
                EditorApplication.QueuePlayerLoopUpdate();
            }

            OnRemoteOperationApplied?.Invoke(op);
        }

        public void ProcessInboundQueue()
        {
            lock (inboundQueue)
            {
                while (inboundQueue.Count > 0)
                {
                    var op = inboundQueue.Dequeue();
                    ApplyRemoteOperation(op);
                }
            }
        }

        private void OnLocalOperationCaptured(CollaborationOperation op)
        {
            if (op == null) return;
            op.ActorId = actorId;
            op.RoomId = CurrentRoomId;

            crdtEngine.ProcessIncomingOperation(op);
            SendOperation(op);
        }

        private void OnNetworkMessageReceived(CollaborationOperation op)
        {
            lock (inboundQueue)
            {
                inboundQueue.Enqueue(op);
            }
        }

        private void ExecuteOperationOnScene(CollaborationOperation op)
        {
            switch (op.OpType)
            {
                case OperationType.CreateGameObject:
                    string name = op.Payload?.StringValue ?? "New GameObject";
                    GameObject newGo;
                    if (name.IndexOf("Cube", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        newGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        newGo.name = name;
                    }
                    else if (name.IndexOf("Sphere", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        newGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        newGo.name = name;
                    }
                    else if (name.IndexOf("Cylinder", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        newGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        newGo.name = name;
                    }
                    else if (name.IndexOf("Plane", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        newGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        newGo.name = name;
                    }
                    else
                    {
                        newGo = new GameObject(name);
                    }

                    ScreenWorkingIdentityManager.Register(newGo, op.TargetObjectId);
                    if (!string.IsNullOrEmpty(op.TargetParentId))
                    {
                        var parentIdentity = ScreenWorkingIdentityManager.FindById(op.TargetParentId);
                        if (parentIdentity != null)
                        {
                            newGo.transform.SetParent(parentIdentity.transform, false);
                        }
                    }
                    Undo.RegisterCreatedObjectUndo(newGo, "[screen working] Create Remote Object");
                    break;

                case OperationType.RenameGameObject:
                    var renameIdentity = ScreenWorkingIdentityManager.FindById(op.TargetObjectId);
                    if (renameIdentity != null && op.Payload != null)
                    {
                        renameIdentity.gameObject.name = op.Payload.StringValue;
                        EditorUtility.SetDirty(renameIdentity.gameObject);
                    }
                    break;

                case OperationType.DestroyGameObject:
                    var destroyIdentity = ScreenWorkingIdentityManager.FindById(op.TargetObjectId);
                    if (destroyIdentity != null)
                    {
                        ScreenWorkingIdentityManager.Unregister(op.TargetObjectId);
                        UnityEngine.Object.DestroyImmediate(destroyIdentity.gameObject);
                    }
                    break;

                case OperationType.ModifyProperty:
                    var targetId = ScreenWorkingIdentityManager.FindById(op.TargetObjectId);
                    if (targetId != null && op.TargetComponentType == "Transform" && op.Payload?.ArrayValues?.Count >= 3)
                    {
                        targetId.transform.localPosition = op.Payload.ArrayValues[0].ToVector3();
                        targetId.transform.localRotation = op.Payload.ArrayValues[1].ToQuaternion();
                        targetId.transform.localScale = op.Payload.ArrayValues[2].ToVector3();
                        EditorUtility.SetDirty(targetId.gameObject);
                    }
                    break;

                case OperationType.ReparentGameObject:
                    var childId = ScreenWorkingIdentityManager.FindById(op.TargetObjectId);
                    if (childId != null)
                    {
                        Transform parentTransform = null;
                        if (!string.IsNullOrEmpty(op.TargetParentId))
                        {
                            var parentId = ScreenWorkingIdentityManager.FindById(op.TargetParentId);
                            if (parentId != null)
                            {
                                parentTransform = parentId.transform;
                            }
                        }
                        childId.transform.SetParent(parentTransform, true);
                        if (op.SiblingIndex >= 0 && op.SiblingIndex < childId.transform.parent?.childCount)
                        {
                            childId.transform.SetSiblingIndex(op.SiblingIndex);
                        }
                        EditorUtility.SetDirty(childId.gameObject);
                    }
                    break;
            }
        }
    }
}
