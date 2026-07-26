using System;
using System.Collections.Generic;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Editor.Engine
{
    /// <summary>
    /// Deterministic Conflict-Free Replicated Data Type (CRDT) engine.
    /// Uses Lamport timestamps + Actor ID metrics, tombstone tracking, and cycle-free reparenting validation.
    /// </summary>
    public class LamportCRDTEngine
    {
        private readonly string actorId;
        private long lamportClock;
        private long actorSequence;

        private readonly Dictionary<string, CollaborationOperation> lastAppliedOperations = new Dictionary<string, CollaborationOperation>();
        private readonly HashSet<string> processedOpIds = new HashSet<string>();
        private readonly HashSet<string> tombstonedObjectIds = new HashSet<string>();
        private readonly List<CollaborationOperation> operationHistory = new List<CollaborationOperation>();

        public string ActorId => actorId;
        public long LamportClock => lamportClock;
        public long ActorSequence => actorSequence;
        public IReadOnlyList<CollaborationOperation> History => operationHistory;

        public LamportCRDTEngine(string actorId)
        {
            this.actorId = actorId ?? Guid.NewGuid().ToString("N");
            this.lamportClock = 0;
            this.actorSequence = 0;
        }

        /// <summary>
        /// Advances local Lamport timestamp for a newly generated local operation.
        /// </summary>
        public CollaborationOperation CreateLocalOperation(OperationType opType, string targetObjectId, SerializedValue payload = null)
        {
            lamportClock++;
            actorSequence++;

            var op = new CollaborationOperation
            {
                ActorId = actorId,
                ActorSequence = actorSequence,
                LamportTimestamp = lamportClock,
                OpType = opType,
                TargetObjectId = targetObjectId,
                Payload = payload
            };

            processedOpIds.Add(op.OperationId);
            operationHistory.Add(op);
            TrackOpState(op);

            return op;
        }

        /// <summary>
        /// Processes an incoming (remote or synced) operation against local CRDT state.
        /// </summary>
        /// <param name="op">The operation to evaluate.</param>
        /// <returns>True if the operation is valid and should be applied locally; false if discarded.</returns>
        public bool ProcessIncomingOperation(CollaborationOperation op)
        {
            if (op == null) return false;

            // Idempotency check: discard already processed operations
            if (processedOpIds.Contains(op.OperationId))
            {
                return false;
            }

            // Advance local Lamport clock
            lamportClock = Math.Max(lamportClock, op.LamportTimestamp) + 1;
            processedOpIds.Add(op.OperationId);

            // Deletion wins check: if object is tombstoned and incoming op is older than tombstone, reject
            if (tombstonedObjectIds.Contains(op.TargetObjectId) && op.OpType != OperationType.RestoreGameObject)
            {
                if (op.OpType != OperationType.DestroyGameObject)
                {
                    return false;
                }
            }

            // Conflict resolution by target property/object
            string stateKey = GetStateKey(op);
            if (lastAppliedOperations.TryGetValue(stateKey, out var existingOp))
            {
                // Compare deterministic ordering
                if (op.CompareDeterministicTo(existingOp) <= 0)
                {
                    // Existing operation is newer or equal priority, discard incoming
                    return false;
                }
            }

            lastAppliedOperations[stateKey] = op;
            operationHistory.Add(op);
            TrackOpState(op);

            return true;
        }

        /// <summary>
        /// Validates that a reparenting operation will not introduce a cyclic hierarchy loop.
        /// </summary>
        public bool ValidateReparentCycle(string childId, string candidateParentId, Func<string, string> getParentFunc)
        {
            if (string.Equals(childId, candidateParentId, StringComparison.Ordinal))
            {
                return false; // Cannot parent to self
            }

            string current = candidateParentId;
            while (!string.IsNullOrEmpty(current))
            {
                if (string.Equals(current, childId, StringComparison.Ordinal))
                {
                    return false; // Cycle detected
                }
                current = getParentFunc != null ? getParentFunc(current) : null;
            }

            return true;
        }

        private void TrackOpState(CollaborationOperation op)
        {
            if (op.OpType == OperationType.DestroyGameObject)
            {
                tombstonedObjectIds.Add(op.TargetObjectId);
            }
            else if (op.OpType == OperationType.RestoreGameObject)
            {
                tombstonedObjectIds.Remove(op.TargetObjectId);
            }
        }

        private string GetStateKey(CollaborationOperation op)
        {
            if (op.OpType == OperationType.DestroyGameObject || op.OpType == OperationType.CreateGameObject)
            {
                return $"OBJ:{op.TargetObjectId}";
            }
            if (!string.IsNullOrEmpty(op.PropertyPath))
            {
                return $"PROP:{op.TargetObjectId}:{op.TargetComponentType}:{op.PropertyPath}";
            }
            return $"OP:{op.TargetObjectId}:{op.OpType}";
        }

        /// <summary>
        /// Computes a hash of the current history state for convergence verification.
        /// </summary>
        public string ComputeStateHash()
        {
            long hash = 17;
            foreach (var op in operationHistory)
            {
                hash = hash * 31 + op.OperationId.GetHashCode();
                hash = hash * 31 + op.LamportTimestamp.GetHashCode();
            }
            return hash.ToString("X16");
        }
    }
}
