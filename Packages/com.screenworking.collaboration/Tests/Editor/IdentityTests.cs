using NUnit.Framework;
using ScreenWorking.Collaboration.Editor.Identity;
using UnityEngine;

namespace ScreenWorking.Collaboration.Tests.Editor
{
    [TestFixture]
    public class IdentityTests
    {
        private GameObject testGo;

        [SetUp]
        public void SetUp()
        {
            testGo = new GameObject("TestObject");
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
        public void Register_AssignsValidUniqueGuid()
        {
            var identity = ScreenWorkingIdentityManager.Register(testGo);
            Assert.IsNotNull(identity);
            Assert.IsFalse(string.IsNullOrEmpty(identity.ObjectId));
            Assert.AreEqual(32, identity.ObjectId.Length); // 32 hex chars GUID
        }

        [Test]
        public void FindById_ReturnsRegisteredObject()
        {
            var identity = ScreenWorkingIdentityManager.Register(testGo);
            var found = ScreenWorkingIdentityManager.FindById(identity.ObjectId);
            Assert.AreEqual(identity, found);
        }

        [Test]
        public void Register_DetectsDuplicateGuid_AndRegenerates()
        {
            var id1 = ScreenWorkingIdentityManager.Register(testGo);
            string originalGuid = id1.ObjectId;

            var testGo2 = new GameObject("TestObject2");
            var id2 = testGo2.AddComponent<ScreenWorkingIdentity>();
            id2.AssignId(originalGuid); // Force duplicate GUID

            ScreenWorkingIdentityManager.Register(testGo2);

            Assert.AreNotEqual(id1.ObjectId, id2.ObjectId);
            Object.DestroyImmediate(testGo2);
        }
    }
}
