using NUnit.Framework;
using ScreenWorking.Collaboration.Editor.Capture;
using ScreenWorking.Collaboration.Editor.Identity;
using ScreenWorking.Collaboration.Editor.Models;
using UnityEngine;

namespace ScreenWorking.Collaboration.Tests.Editor
{
    [TestFixture]
    public class ChangeTrackerTests
    {
        private ScreenWorkingChangeTracker tracker;
        private GameObject testGo;

        [SetUp]
        public void SetUp()
        {
            tracker = new ScreenWorkingChangeTracker();
            testGo = new GameObject("TrackerObject");
        }

        [TearDown]
        public void TearDown()
        {
            if (testGo != null)
            {
                Object.DestroyImmediate(testGo);
            }
            ScreenWorkingIdentityManager.ClearRegistry();
        }

        [Test]
        public void RecordCreateGameObject_GeneratesCorrectOperation()
        {
            CollaborationOperation capturedOp = null;
            tracker.OnOperationCaptured += op => capturedOp = op;

            tracker.RecordCreateGameObject(testGo);

            Assert.IsNotNull(capturedOp);
            Assert.AreEqual(OperationType.CreateGameObject, capturedOp.OpType);
            Assert.AreEqual("TrackerObject", capturedOp.Payload.StringValue);
        }

        [Test]
        public void SyncScope_SuppressesLocalChangeCapture()
        {
            CollaborationOperation capturedOp = null;
            tracker.OnOperationCaptured += op => capturedOp = op;

            using (ScreenWorkingSyncScope.SuppressLocalCapture())
            {
                tracker.RecordRenameGameObject(testGo, "NewNameSuppressed");
            }

            Assert.IsNull(capturedOp, "Operation should be suppressed when inside ScreenWorkingSyncScope.");
        }
    }
}
