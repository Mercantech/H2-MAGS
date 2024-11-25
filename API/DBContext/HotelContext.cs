using DomainModels;

namespace API.DBContext
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Threading;
    using System.Threading.Tasks;

    public class HotelContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }

        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingUser>()
                .HasKey(bu => new { bu.BookingId, bu.UserId });

            modelBuilder.Entity<BookingUser>()
                .HasOne(bu => bu.Booking)
                .WithMany(b => b.BookingUsers)
                .HasForeignKey(bu => bu.BookingId);

            modelBuilder.Entity<BookingUser>()
                .HasOne(bu => bu.User)
            .WithMany(u => u.BookingUsers)
            .HasForeignKey(bu => bu.UserId);

            modelBuilder.Entity<BookingRoom>()
                .HasKey(br => new { br.BookingId, br.RoomId });

            modelBuilder.Entity<BookingRoom>()
            .HasOne(br => br.Booking)
            .WithMany(b => b.BookingRooms)
            .HasForeignKey(br => br.BookingId);

            modelBuilder.Entity<BookingRoom>()
            .HasOne(br => br.Room)
            .WithMany(r => r.BookingRooms)  
            .HasForeignKey(br => br.RoomId);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Common &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (Common)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = Guid.NewGuid().ToString();
                    }
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }


}
