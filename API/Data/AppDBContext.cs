using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserInfo> UserInfos { get; set; } = null!;
        public DbSet<Hotel> Hotels { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        
        // Ticket System entities
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketComment> TicketComments { get; set; } = null!;
        public DbSet<TicketAttachment> TicketAttachments { get; set; } = null!;
        public DbSet<TicketHistory> TicketHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurer Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                // Navn skal være unikt
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // Konfigurer User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Email skal være unikt
                entity.HasIndex(u => u.Email).IsUnique();
                
                // Konfigurer foreign key til Role
                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserInfo>()
                .HasKey(i => i.UserId); // Shared PK

            modelBuilder.Entity<User>()
                .HasOne(u => u.Info)
                .WithOne(i => i.User)
                .HasForeignKey<UserInfo>(i => i.UserId);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId);

            // Konfigurer Ticket entities
            ConfigureTicketEntities(modelBuilder);

            // Seed roller og test brugere (kun til udvikling)
            SeedRoles(modelBuilder);
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {

            var roles = new[]
            {
                new Role
                {
                    // Nyt tilfældigt guid
                    Id = "1",
                    Name = "User",
                    Description = "Standard bruger med basis rettigheder",
                    CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = "2",
                    Name = "CleaningStaff",
                    Description = "Rengøringspersonale med adgang til rengøringsmoduler",
                    CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = "3",
                    Name = "Reception",
                    Description = "Receptionspersonale med adgang til booking og gæster",
                    CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = "4",
                    Name = "Admin",
                    Description = "Administrator med fuld adgang til systemet",
                    CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            };

            modelBuilder.Entity<Role>().HasData(roles);
        }

        private void ConfigureTicketEntities(ModelBuilder modelBuilder)
        {
            // Konfigurer Ticket entity
            modelBuilder.Entity<Ticket>(entity =>
            {
                // Ticket number skal være unikt
                entity.HasIndex(t => t.TicketNumber).IsUnique();
                
                // Konfigurer foreign keys
                entity.HasOne(t => t.Requester)
                    .WithMany()
                    .HasForeignKey(t => t.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Assignee)
                    .WithMany()
                    .HasForeignKey(t => t.AssigneeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Booking)
                    .WithMany()
                    .HasForeignKey(t => t.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Room)
                    .WithMany()
                    .HasForeignKey(t => t.RoomId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Hotel)
                    .WithMany()
                    .HasForeignKey(t => t.HotelId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Konfigurer TicketComment entity
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.HasOne(tc => tc.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(tc => tc.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tc => tc.Author)
                    .WithMany()
                    .HasForeignKey(tc => tc.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Konfigurer TicketAttachment entity
            modelBuilder.Entity<TicketAttachment>(entity =>
            {
                entity.HasOne(ta => ta.Ticket)
                    .WithMany(t => t.Attachments)
                    .HasForeignKey(ta => ta.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ta => ta.UploadedBy)
                    .WithMany()
                    .HasForeignKey(ta => ta.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Konfigurer TicketHistory entity
            modelBuilder.Entity<TicketHistory>(entity =>
            {
                entity.HasOne(th => th.Ticket)
                    .WithMany(t => t.History)
                    .HasForeignKey(th => th.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(th => th.ChangedBy)
                    .WithMany()
                    .HasForeignKey(th => th.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
