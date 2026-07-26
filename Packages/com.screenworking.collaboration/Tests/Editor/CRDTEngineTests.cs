using NUnit.Framework;
using ScreenWorking.Collaboration.Editor.Engine;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Tests.Editor
{
    [TestFixture]
    public class CRDTEngineTests
    {
        private LamportCRDTEngine crdtEngine;

        [SetUp]
        public void SetUp()
        {
            crdtEngine = new LamportCRDTEngine("actor-client-1");
        }

        [Test]
        public void CreateLocalOperation_AdvancesLamportClockAndSequence()
        {
            var op1 = crdtEngine.CreateLocalOperation(OperationType.CreateGameObject, "obj-101", SerializedValue.FromString("Cube"));
            Assert.AreEqual(1, op1.LamportTimestamp);
            Assert.AreEqual(1, op1.ActorSequence);

            var op2 = crdtEngine.CreateLocalOperation(OperationType.RenameGameObject, "obj-101", SerializedValue.FromString("MyCube"));
            Assert.AreEqual(2, op2.LamportTimestamp);
            Assert.AreEqual(2, op2.ActorSequence);
        }

        [Test]
        public void Idempotency_DiscardsDuplicateOperations()
        {
            var op = crdtEngine.CreateLocalOperation(OperationType.CreateGameObject, "obj-102", SerializedValue.FromString("Sphere"));
            bool reApplied = crdtEngine.ProcessIncomingOperation(op);

            Assert.IsFalse(reApplied, "Duplicate operation must be discarded.");
        }

        [Test]
        public void ReparentCycle_RejectsCyclicHierarchy()
        {
            // Object A is parent of Object B
            bool isValid = crdtEngine.ValidateReparentCycle("obj-A", "obj-B", childId =>
            {
                if (childId == "obj-B") return "obj-A";
                return null;
            });

            Assert.IsFalse(isValid, "Attempting to parent Object A to Object B when B is already child of A must fail cycle validation.");
        }
    }
}
