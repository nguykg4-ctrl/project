using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenWorking.Server.API.Data;
using ScreenWorking.Server.API.Data.Entities;

namespace ScreenWorking.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly CollaborationDbContext dbContext;

        public RoomsController(CollaborationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await dbContext.Rooms.ToListAsync();
            return Ok(rooms);
        }

        public class CreateRoomRequest
        {
            public string Name { get; set; } = string.Empty;
            public string ProjectId { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var room = new RoomEntity
            {
                RoomId = Guid.NewGuid().ToString("N")[..8],
                Name = request.Name,
                ProjectId = request.ProjectId
            };

            dbContext.Rooms.Add(room);
            await dbContext.SaveChangesAsync();

            return Ok(room);
        }
    }
}
