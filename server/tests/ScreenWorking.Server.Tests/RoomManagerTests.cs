using NUnit.Framework;
using ScreenWorking.Server.API.Services;

namespace ScreenWorking.Server.Tests
{
    [TestFixture]
    public class RoomManagerTests
    {
        private RoomManager roomManager;

        [SetUp]
        public void SetUp()
        {
            roomManager = new RoomManager();
        }

        [Test]
        public void GetOrCreateRoom_CreatesUniqueSession()
        {
            var session1 = roomManager.GetOrCreateRoom("room-alpha");
            var session2 = roomManager.GetOrCreateRoom("room-alpha");

            Assert.IsNotNull(session1);
            Assert.AreEqual("room-alpha", session1.RoomId);
            Assert.AreSame(session1, session2, "Subsequent calls for the same room ID must return the existing session instance.");
        }
    }
}
