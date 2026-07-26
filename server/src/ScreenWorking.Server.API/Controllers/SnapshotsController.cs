using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenWorking.Server.API.Data;
using ScreenWorking.Server.API.Data.Entities;

namespace ScreenWorking.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SnapshotsController : ControllerBase
    {
        private readonly CollaborationDbContext dbContext;

        public SnapshotsController(CollaborationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("{roomId}/latest")]
        public async Task<IActionResult> GetLatestSnapshot(string roomId)
        {
            var snapshot = await dbContext.Snapshots
                .Where(s => s.RoomId == roomId)
                .OrderByDescending(s => s.SequenceNumber)
                .FirstOrDefaultAsync();

            if (snapshot == null)
            {
                return NotFound("No snapshot exists for this room.");
            }

            return Ok(snapshot);
        }

        public class CreateSnapshotRequest
        {
            public long SequenceNumber { get; set; }
            public byte[] Payload { get; set; } = Array.Empty<byte>();
        }

        [HttpPost("{roomId}")]
        public async Task<IActionResult> CreateSnapshot(string roomId, [FromBody] CreateSnapshotRequest request)
        {
            var snapshot = new SnapshotEntity
            {
                RoomId = roomId,
                SequenceNumber = request.SequenceNumber,
                Payload = request.Payload
            };

            dbContext.Snapshots.Add(snapshot);
            await dbContext.SaveChangesAsync();

            return Ok(snapshot);
        }
    }
}
