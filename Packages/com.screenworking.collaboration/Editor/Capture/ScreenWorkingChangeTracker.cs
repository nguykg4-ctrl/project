using System;
using System.Collections.Generic;
using ScreenWorking.Collaboration.Editor.Identity;
using ScreenWorking.Collaboration.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace ScreenWorking.Collaboration.Editor.Capture
{
    /// <summary>
    /// Captures local Unity Editor scene modifications and converts them into collaboration operations.
    /// Safely guarded by <see cref="ScreenWorkingSyncScope"/> to suppress echo loops.
    /// </summary>
    public class ScreenWorkingChangeTracker
    {
        public event Action<CollaborationOperation> OnOperationCaptured;

        private bool isTracking;

        /// <summary>
        /// Starts capturing Unity Editor scene changes.
        /// </summary>
        public void StartTracking()
        {
            if (isTracking) return;
            isTracking = true;

#if UNITY_2022_3_OR_NEWER
            ObjectChangeEvents.changesPublished += OnObjectChangesPublished;
#endif
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.postprocessModifications += OnPostprocessModifications;
        }

        /// <summary>
        /// Stops capturing Unity Editor scene changes.
        /// </summary>
        public void StopTracking()
        {
            if (!isTracking) return;
            isTracking = false;

#if UNITY_2022_3_OR_NEWER
            ObjectChangeEvents.changesPublished -= OnObjectChangesPublished;
#endif
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.postprocessModifications -= OnPostprocessModifications;
        }

        /// <summary>
        /// Manually captures a GameObject creation operation.
        /// </summary>
        public CollaborationOperation RecordCreateGameObject(GameObject go, string parentId = null)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture || go == null) return null;

            string id = ScreenWorkingIdentityManager.GetOrCreateId(go);
            var op = new CollaborationOperation
            {
                OpType = OperationType.CreateGameObject,
                TargetObjectId = id,
                TargetParentId = parentId,
                Payload = SerializedValue.FromString(go.name)
            };

            OnOperationCaptured?.Invoke(op);
            return op;
        }

        /// <summary>
        /// Manually captures a GameObject rename operation.
        /// </summary>
        public CollaborationOperation RecordRenameGameObject(GameObject go, string newName)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture || go == null) return null;

            string id = ScreenWorkingIdentityManager.GetOrCreateId(go);
            var op = new CollaborationOperation
            {
                OpType = OperationType.RenameGameObject,
                TargetObjectId = id,
                Payload = SerializedValue.FromString(newName)
            };

            OnOperationCaptured?.Invoke(op);
            return op;
        }

        /// <summary>
        /// Manually captures a Transform modification.
        /// </summary>
        public CollaborationOperation RecordTransformChange(GameObject go, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture || go == null) return null;

            string id = ScreenWorkingIdentityManager.GetOrCreateId(go);
            var payload = new SerializedValue
            {
                ValueType = SerializedValueType.Array,
                ArrayValues = new List<SerializedValue>
                {
                    SerializedValue.FromVector3(pos),
                    SerializedValue.FromQuaternion(rot),
                    SerializedValue.FromVector3(scale)
                }
            };

            var op = new CollaborationOperation
            {
                OpType = OperationType.ModifyProperty,
                TargetObjectId = id,
                TargetComponentType = "Transform",
                PropertyPath = "transformState",
                Payload = payload
            };

            OnOperationCaptured?.Invoke(op);
            return op;
        }

        /// <summary>
        /// Manually captures a GameObject deletion.
        /// </summary>
        public CollaborationOperation RecordDestroyGameObject(string objectId)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture || string.IsNullOrEmpty(objectId)) return null;

            var op = new CollaborationOperation
            {
                OpType = OperationType.DestroyGameObject,
                TargetObjectId = objectId
            };

            OnOperationCaptured?.Invoke(op);
            return op;
        }

        /// <summary>
        /// Manually captures a reparenting operation.
        /// </summary>
        public CollaborationOperation RecordReparentGameObject(GameObject go, GameObject newParent, int siblingIndex = -1)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture || go == null) return null;

            string id = ScreenWorkingIdentityManager.GetOrCreateId(go);
            string parentId = newParent != null ? ScreenWorkingIdentityManager.GetOrCreateId(newParent) : null;

            var op = new CollaborationOperation
            {
                OpType = OperationType.ReparentGameObject,
                TargetObjectId = id,
                TargetParentId = parentId,
                SiblingIndex = siblingIndex
            };

            OnOperationCaptured?.Invoke(op);
            return op;
        }

#if UNITY_2022_3_OR_NEWER
        private void OnObjectChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture) return;
        }
#endif

        private UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture || modifications == null) return modifications;

            foreach (var mod in modifications)
            {
                if (mod.currentValue.target is Transform t && t != null)
                {
                    RecordTransformChange(t.gameObject, t.localPosition, t.localRotation, t.localScale);
                    break;
                }
            }
            return modifications;
        }

        private void OnHierarchyChanged()
        {
            if (ScreenWorkingSyncScope.IsSuppressingLocalCapture) return;

            if (Selection.activeGameObject != null)
            {
                var targetObj = Selection.activeGameObject;
                RecordTransformChange(targetObj, targetObj.transform.localPosition, targetObj.transform.localRotation, targetObj.transform.localScale);
            }
        }
    }
}
