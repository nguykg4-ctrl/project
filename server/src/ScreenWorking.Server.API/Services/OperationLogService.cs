using Microsoft.EntityFrameworkCore;
using ScreenWorking.Server.API.Data;
using ScreenWorking.Server.API.Data.Entities;

namespace ScreenWorking.Server.API.Services
{
    public class OperationLogService
    {
        private readonly CollaborationDbContext dbContext;

        public OperationLogService(CollaborationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<long> LogOperationAsync(string roomId, string opId, string actorId, int opType, string targetId, byte[] payload)
        {
            var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null)
            {
                room = new RoomEntity { RoomId = roomId, Name = roomId };
                dbContext.Rooms.Add(room);
            }

            room.CurrentSequence++;

            var entity = new OperationEntity
            {
                RoomId = roomId,
                OperationId = opId,
                ActorId = actorId,
                ServerSequence = room.CurrentSequence,
                OpType = opType,
                TargetObjectId = targetId,
                Payload = payload
            };

            dbContext.Operations.Add(entity);
            await dbContext.SaveChangesAsync();

            return room.CurrentSequence;
        }

        public async Task<List<OperationEntity>> GetOperationsSinceAsync(string roomId, long sinceSequence)
        {
            return await dbContext.Operations
                .Where(o => o.RoomId == roomId && o.ServerSequence > sinceSequence)
                .OrderBy(o => o.ServerSequence)
                .ToListAsync();
        }
    }
}
