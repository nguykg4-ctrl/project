using NUnit.Framework;
using ScreenWorking.Collaboration.Editor.Models;
using ScreenWorking.Collaboration.Shared.Protocol;

namespace ScreenWorking.Server.Tests
{
    [TestFixture]
    public class ProtocolValidationTests
    {
        [Test]
        public void SerializeAndDeserialize_PreservesOperationData()
        {
            var op = new CollaborationOperation
            {
                ProtocolVersion = 1,
                ProjectId = "proj-unity-2026",
                RoomId = "room-level-1",
                ActorId = "actor-user-42",
                OpType = OperationType.ModifyProperty,
                TargetObjectId = "guid-obj-777",
                TargetComponentType = "Transform",
                PropertyPath = "transformState",
                Payload = SerializedValue.FromVector3(10f, 20f, 30f)
            };

            byte[] serialized = MessagePackSerializerWrapper.Serialize(op);
            Assert.IsNotNull(serialized);
            Assert.Greater(serialized.Length, 0);

            var deserialized = MessagePackSerializerWrapper.Deserialize(serialized);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(op.OperationId, deserialized.OperationId);
            Assert.AreEqual(op.ActorId, deserialized.ActorId);
            Assert.AreEqual(op.OpType, deserialized.OpType);
            Assert.AreEqual(op.TargetObjectId, deserialized.TargetObjectId);
        }
    }
}
