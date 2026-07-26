using Microsoft.EntityFrameworkCore;
using ScreenWorking.Server.API.Data.Entities;

namespace ScreenWorking.Server.API.Data
{
    public class CollaborationDbContext : DbContext
    {
        public CollaborationDbContext(DbContextOptions<CollaborationDbContext> options) : base(options) { }

        public DbSet<RoomEntity> Rooms { get; set; } = null!;
        public DbSet<UserEntity> Users { get; set; } = null!;
        public DbSet<SnapshotEntity> Snapshots { get; set; } = null!;
        public DbSet<OperationEntity> Operations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OperationEntity>().HasIndex(o => new { o.RoomId, o.ServerSequence });
            modelBuilder.Entity<OperationEntity>().HasIndex(o => o.OperationId).IsUnique();
        }
    }
}
