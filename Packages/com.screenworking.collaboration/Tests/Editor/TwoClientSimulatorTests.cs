using NUnit.Framework;
using ScreenWorking.Collaboration.Editor.Engine;
using ScreenWorking.Collaboration.Editor.Identity;
using ScreenWorking.Collaboration.Editor.Models;
using ScreenWorking.Collaboration.Editor.Transport;
using UnityEngine;

namespace ScreenWorking.Collaboration.Tests.Editor
{
    [TestFixture]
    public class TwoClientSimulatorTests
    {
        private CollaborationClientEngine clientA;
        private CollaborationClientEngine clientB;
        private MemoryWebSocketChannel channelA;
        private MemoryWebSocketChannel channelB;

        [SetUp]
        public void SetUp()
        {
            MemoryWebSocketChannel.ResetBrokers();
            ScreenWorkingIdentityManager.ClearRegistry();

            channelA = new MemoryWebSocketChannel();
            channelB = new MemoryWebSocketChannel();

            clientA = new CollaborationClientEngine("Client-A", channelA);
            clientB = new CollaborationClientEngine("Client-B", channelB);

            clientA.Connect("ws://local", "test-room-1", "token-a");
            clientB.Connect("ws://local", "test-room-1", "token-b");
        }

        [TearDown]
        public void TearDown()
        {
            clientA?.Disconnect();
            clientB?.Disconnect();
            MemoryWebSocketChannel.ResetBrokers();
            ScreenWorkingIdentityManager.ClearRegistry();
        }

        [Test]
        public void TwoClients_SynchronizeGameObjectCreation()
        {
            // Client A creates a GameObject
            var goA = new GameObject("CubeFromA");
            string idA = ScreenWorkingIdentityManager.GetOrCreateId(goA);

            var createOp = clientA.CRDTEngine.CreateLocalOperation(OperationType.CreateGameObject, idA, SerializedValue.FromString("CubeFromA"));
            clientA.SendOperation(createOp);

            // Client B processes inbound messages
            clientB.ProcessInboundQueue();

            Assert.AreEqual(clientA.CRDTEngine.History.Count, clientB.CRDTEngine.History.Count);

            Object.DestroyImmediate(goA);
        }

        [Test]
        public void ConcurrentPropertyEdits_ConvergeToIdenticalStateHash()
        {
            string sharedObjId = "shared-object-999";

            // Client A performs transform modification at Lamport T1
            var opA = clientA.CRDTEngine.CreateLocalOperation(OperationType.ModifyProperty, sharedObjId, SerializedValue.FromVector3(new Vector3(1, 2, 3)));
            clientA.SendOperation(opA);

            // Client B performs transform modification at Lamport T1
            var opB = clientB.CRDTEngine.CreateLocalOperation(OperationType.ModifyProperty, sharedObjId, SerializedValue.FromVector3(new Vector3(10, 20, 30)));
            clientB.SendOperation(opB);

            // Both process incoming messages
            clientA.ProcessInboundQueue();
            clientB.ProcessInboundQueue();

            // Assert identical state hash across both CRDT engines
            string hashA = clientA.CRDTEngine.ComputeStateHash();
            string hashB = clientB.CRDTEngine.ComputeStateHash();

            Assert.AreEqual(hashA, hashB, "Client A and Client B must converge to the exact same CRDT state hash.");
        }
    }
}
