using API.Data;
using Bogus;
using DomainModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    /// <summary>
    /// Database statistikker model.
    /// </summary>
    public class DatabaseStats
    {
        public int Users { get; set; }
        public int AdminUsers { get; set; }
        public int Hotels { get; set; }
        public int Rooms { get; set; }
        public int Bookings { get; set; }
        public int ActiveBookings { get; set; }
        public DateTime LastSeeded { get; set; }
    }
    /// <summary>
    /// Service til at seede databasen med test data ved hjælp af Bogus faker library.
    /// Genererer realistiske test data for brugere, hoteller, rum og bookinger.
    /// </summary>
    public class DataSeederService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DataSeederService> _logger;

        public DataSeederService(AppDBContext context, ILogger<DataSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeder databasen med komplet test data.
        /// </summary>
        /// <param name="userCount">Antal brugere at oprette</param>
        /// <param name="hotelCount">Antal hoteller at oprette</param>
        /// <param name="roomsPerHotel">Antal rum per hotel</param>
        /// <param name="bookingCount">Antal bookinger at oprette</param>
        public async Task<string> SeedDatabaseAsync(int userCount = 50, int hotelCount = 10, int roomsPerHotel = 20, int bookingCount = 100)
        {
            try
            {
                var summary = new StringBuilder();
                _logger.LogInformation("Starter database seeding...");

                // Tjek om der allerede er data
                var existingUsers = await _context.Users.CountAsync();
                var existingHotels = await _context.Hotels.CountAsync();
                var existingRooms = await _context.Rooms.CountAsync();
                var existingBookings = await _context.Bookings.CountAsync();

                summary.AppendLine($"Eksisterende data før seeding:");
                summary.AppendLine($"- Brugere: {existingUsers}");
                summary.AppendLine($"- Hoteller: {existingHotels}");
                summary.AppendLine($"- Rum: {existingRooms}");
                summary.AppendLine($"- Bookinger: {existingBookings}");
                summary.AppendLine();

                // Sikr at der findes roller i databasen
                await EnsureRolesExistAsync();
                summary.AppendLine("✅ Roller sikret");

                // Seed brugere
                var users = await SeedUsersAsync(userCount);
                summary.AppendLine($"✅ Oprettet {users.Count} brugere");

                // Seed hoteller
                var hotels = await SeedHotelsAsync(hotelCount);
                summary.AppendLine($"✅ Oprettet {hotels.Count} hoteller");

                // Seed rum
                var rooms = await SeedRoomsAsync(hotels, roomsPerHotel);
                summary.AppendLine($"✅ Oprettet {rooms.Count} rum");

                // Seed bookinger
                var bookings = await SeedBookingsAsync(users, rooms, bookingCount);
                summary.AppendLine($"✅ Oprettet {bookings.Count} bookinger");

                summary.AppendLine();
                summary.AppendLine("🎉 Database seeding fuldført succesfuldt!");

                _logger.LogInformation("Database seeding fuldført succesfuldt");
                return summary.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under database seeding");
                throw;
            }
        }

        /// <summary>
        /// Tjekker at de nødvendige roller eksisterer i databasen.
        /// </summary>
        private async Task EnsureRolesExistAsync()
        {
            var existingRoles = await _context.Roles.ToListAsync();
            
            _logger.LogInformation("Fundet {RoleCount} roller i databasen: {RoleNames}", 
                existingRoles.Count, 
                string.Join(", ", existingRoles.Select(r => r.Name)));

            // Tjek om User og Admin roller eksisterer
            var userRole = existingRoles.FirstOrDefault(r => r.Name == "User");
            var adminRole = existingRoles.FirstOrDefault(r => r.Name == "Admin");

            if (userRole == null)
            {
                throw new InvalidOperationException("User rolle ikke fundet i databasen. Kør database migrations først.");
            }

            if (adminRole == null)
            {
                throw new InvalidOperationException("Admin rolle ikke fundet i databasen. Kør database migrations først.");
            }
        }

        /// <summary>
        /// Opretter fake brugere med forskellige roller.
        /// </summary>
        private async Task<List<User>> SeedUsersAsync(int count)
        {
            var existingUsers = await _context.Users.Select(u => u.Email).ToListAsync();
            var users = new List<User>();

            // Hent faktiske rolle ID'er fra databasen
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            if (userRole == null || adminRole == null)
            {
                throw new InvalidOperationException("User eller Admin rolle ikke fundet i databasen");
            }

            // Danske navne til at gøre data mere realistisk
            var danishFirstNames = new[]
            {
                "Anders", "Anne", "Bo", "Birgitte", "Christian", "Charlotte", "Daniel", "Dorthe",
                "Erik", "Eva", "Frederik", "Freja", "Henrik", "Helle", "Jacob", "Janne",
                "Klaus", "Karen", "Lars", "Lone", "Mads", "Maria", "Niels", "Nina",
                "Ole", "Pia", "Peter", "Rikke", "Søren", "Susanne", "Thomas", "Tina"
            };

            var danishLastNames = new[]
            {
                "Andersen", "Nielsen", "Hansen", "Pedersen", "Jørgensen", "Larsen", "Sørensen",
                "Rasmussen", "Petersen", "Christensen", "Thomsen", "Olsen", "Madsen", "Møller",
                "Johansen", "Christiansen", "Jensen", "Kristensen", "Knudsen", "Mortensen"
            };

            var faker = new Faker<User>("en")
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Email, f => 
                {
                    var firstName = f.PickRandom(danishFirstNames);
                    var lastName = f.PickRandom(danishLastNames);
                    return $"{firstName.ToLower()}.{lastName.ToLower()}@{f.PickRandom("gmail.com", "hotmail.com", "yahoo.dk", "outlook.dk")}";
                })
                .RuleFor(u => u.Username, (f, u) => u.Email.Split('@')[0])
                .RuleFor(u => u.HashedPassword, f => HashPassword("Password123!"))
                .RuleFor(u => u.PasswordBackdoor, f => "Password123!")
                .RuleFor(u => u.RoleId, f => f.PickRandom(userRole.Id, userRole.Id, userRole.Id, userRole.Id, adminRole.Id)) // 80% User, 20% Admin
                .RuleFor(u => u.LastLogin, f => f.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow))
                .RuleFor(u => u.UserInfoId, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow))
                .RuleFor(u => u.UpdatedAt, (f, u) => f.Date.Between(u.CreatedAt, DateTime.UtcNow));

            // Generer brugere og filtrér dubletter
            var attempts = 0;
            while (users.Count < count && attempts < count * 2)
            {
                var user = faker.Generate();
                if (!existingUsers.Contains(user.Email) && !users.Any(u => u.Email == user.Email))
                {
                    users.Add(user);
                }
                attempts++;
            }

            // Tilføj en garanteret admin bruger
            if (!existingUsers.Contains("admin@hotel.dk"))
            {
                users.Add(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "admin@hotel.dk",
                    Username = "admin",
                    HashedPassword = HashPassword("Admin123!"),
                    PasswordBackdoor = "Admin123!",
                    RoleId = adminRole.Id,
                    UserInfoId = Guid.NewGuid().ToString(),
                    LastLogin = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-365),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                });
            }

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            return users;
        }

        /// <summary>
        /// Opretter fake hoteller med realistiske data.
        /// </summary>
        private async Task<List<Hotel>> SeedHotelsAsync(int count)
        {
            var hotels = new List<Hotel>();

            var hotelNames = new[]
            {
                "Hotel Royal", "Grand Hotel", "Scandic", "Best Western", "Radisson Blu",
                "Hotel Alexandra", "Nimb Hotel", "Hotel d'Angleterre", "Copenhagen Marriott",
                "Clarion Hotel", "Comfort Hotel", "First Hotel", "Cabinn Hotel", "Wakeup Hotel",
                "Hotel Phoenix", "Hotel Kong Arthur", "Hotel Sanders", "71 Nyhavn Hotel",
                "Hotel Skt. Petri", "AC Hotel", "Villa Copenhagen", "Hotel SP34"
            };

            var danishCities = new[]
            {
                "København", "Aarhus", "Odense", "Aalborg", "Esbjerg",
                "Randers", "Kolding", "Horsens", "Vejle", "Roskilde",
                "Herning", "Silkeborg", "Næstved", "Fredericia", "Viborg"
            };

            var danishStreets = new[]
            {
                "Nørregade", "Vestergade", "Østergade", "Søndergade", "Hovedgade",
                "Kongens Nytorv", "Strøget", "Nyhavn", "Amaliegade", "Bredgade",
                "Store Kongensgade", "Gothersgade", "Sankt Peders Stræde"
            };

            for (int i = 0; i < count; i++)
            {
                var baseFaker = new Faker();
                var hotelName = baseFaker.PickRandom(hotelNames) + " " + baseFaker.PickRandom(danishCities);
                
                // Sikr unikt navn
                var counter = 1;
                var originalName = hotelName;
                while (hotels.Any(h => h.Name == hotelName))
                {
                    hotelName = originalName + " " + counter;
                    counter++;
                }

                var hotel = new Hotel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = hotelName,
                    Address = baseFaker.PickRandom(danishStreets) + " " + baseFaker.Random.Int(1, 200) + ", " + baseFaker.Random.Int(1000, 9999) + " " + baseFaker.PickRandom(danishCities) + ", Danmark",
                    CreatedAt = baseFaker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow.AddYears(-1)),
                    UpdatedAt = DateTime.UtcNow
                };
                
                hotel.UpdatedAt = baseFaker.Date.Between(hotel.CreatedAt, DateTime.UtcNow);
                hotels.Add(hotel);
            }

            _context.Hotels.AddRange(hotels);
            await _context.SaveChangesAsync();

            return hotels;
        }

        /// <summary>
        /// Opretter fake rum for hvert hotel.
        /// </summary>
        private async Task<List<Room>> SeedRoomsAsync(List<Hotel> hotels, int roomsPerHotel)
        {
            var rooms = new List<Room>();
            var roomTypes = new[] { "Standard", "Deluxe", "Suite", "Junior Suite", "Presidential Suite", "Family Room", "Single", "Double", "Twin" };

            foreach (var hotel in hotels)
            {
                var faker = new Faker<Room>("en")
                    .RuleFor(r => r.Id, f => Guid.NewGuid().ToString())
                    .RuleFor(r => r.Number, f => f.Random.Int(101, 999).ToString())
                    .RuleFor(r => r.Capacity, f => f.Random.WeightedRandom(new[] { 1, 2, 3, 4, 6 }, new[] { 0.1f, 0.5f, 0.2f, 0.15f, 0.05f }))
                    .RuleFor(r => r.NumberOfBeds, (f, r) => r.Capacity <= 2 ? f.Random.Int(1, 2) : f.Random.Int(2, r.Capacity))
                    .RuleFor(r => r.RoomType, f => f.PickRandom(roomTypes))
                    .RuleFor(r => r.PricePerNight, f => f.Random.Decimal(500, 3000))
                    .RuleFor(r => r.FloorNumber, f => f.Random.Int(1, 15))
                    .RuleFor(r => r.HasBalcony, f => f.Random.Bool(0.4f))
                    .RuleFor(r => r.HasSeaView, f => f.Random.Bool(0.3f))
                    .RuleFor(r => r.HasWifi, f => f.Random.Bool(0.95f))
                    .RuleFor(r => r.HasAirConditioning, f => f.Random.Bool(0.8f))
                    .RuleFor(r => r.HasMinibar, f => f.Random.Bool(0.6f))
                    .RuleFor(r => r.IsAccessible, f => f.Random.Bool(0.1f))
                    .RuleFor(r => r.Description, f => f.Lorem.Paragraphs(1, 2))
                    .RuleFor(r => r.SquareMeters, f => f.Random.Int(15, 80))
                    .RuleFor(r => r.HotelId, f => hotel.Id)
                    .RuleFor(r => r.CreatedAt, f => f.Date.Between(hotel.CreatedAt, DateTime.UtcNow))
                    .RuleFor(r => r.UpdatedAt, (f, r) => f.Date.Between(r.CreatedAt, DateTime.UtcNow));

                var hotelRooms = faker.Generate(roomsPerHotel);
                
                // Sikr unikke rum numre per hotel
                var usedNumbers = new HashSet<string>();
                foreach (var room in hotelRooms)
                {
                    while (usedNumbers.Contains(room.Number))
                    {
                        room.Number = new Faker().Random.Int(101, 999).ToString();
                    }
                    usedNumbers.Add(room.Number);
                }

                rooms.AddRange(hotelRooms);
            }

            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();

            return rooms;
        }

        /// <summary>
        /// Opretter fake bookinger med realistiske datoer og ingen overlaps.
        /// </summary>
        private async Task<List<Booking>> SeedBookingsAsync(List<User> users, List<Room> rooms, int count)
        {
            var bookings = new List<Booking>();
            var bookingStatuses = new[] { "Confirmed", "Pending", "Cancelled", "CheckedIn", "CheckedOut", "NoShow" };
            var faker = new Faker("en");

            // Opret bookinger med forsigtig overlap håndtering
            for (int i = 0; i < count; i++)
            {
                var user = faker.PickRandom(users);
                var room = faker.PickRandom(rooms);
                
                // Generer realistiske datoer
                var startDate = faker.Date.Between(DateTime.UtcNow.AddDays(-180), DateTime.UtcNow.AddDays(180));
                var nights = faker.Random.WeightedRandom(new[] { 1, 2, 3, 4, 5, 7, 14 }, new[] { 0.1f, 0.3f, 0.25f, 0.15f, 0.1f, 0.08f, 0.02f });
                var endDate = startDate.AddDays(nights);

                // Tjek for overlap (simpel check - kan optimeres)
                var hasOverlap = bookings.Any(b => 
                    b.RoomId == room.Id && 
                    b.BookingStatus != "Cancelled" &&
                    b.StartDate < endDate && 
                    b.EndDate > startDate);

                if (!hasOverlap)
                {
                    var booking = new Booking
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = user.Id,
                        RoomId = room.Id,
                        StartDate = startDate,
                        EndDate = endDate,
                        NumberOfGuests = faker.Random.Int(1, Math.Min(room.Capacity, 4)),
                        TotalPrice = room.PricePerNight * nights,
                        BookingStatus = faker.PickRandom(bookingStatuses),
                        SpecialRequests = faker.Random.Bool(0.3f) ? faker.Lorem.Sentence() : null,
                        CheckInTime = startDate < DateTime.UtcNow ? faker.Date.Between(startDate.AddHours(14), startDate.AddHours(18)) : null,
                        CheckOutTime = endDate < DateTime.UtcNow ? faker.Date.Between(endDate.AddHours(8), endDate.AddHours(12)) : null,
                        CreatedAt = faker.Date.Between(startDate.AddDays(-30), startDate.AddDays(-1)),
                        UpdatedAt = faker.Date.Between(startDate.AddDays(-10), DateTime.UtcNow)
                    };

                    bookings.Add(booking);
                }
            }

            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();

            return bookings;
        }

        /// <summary>
        /// Seeder kun bookinger baseret på eksisterende brugere og rum.
        /// </summary>
        /// <param name="bookingCount">Antal bookinger at oprette</param>
        /// <returns>Seeding resultat</returns>
        public async Task<string> SeedBookingsOnlyAsync(int bookingCount = 50)
        {
            try
            {
                var summary = new StringBuilder();
                _logger.LogInformation("Starter booking-only seeding...");

                // Hent eksisterende brugere og rum
                var existingUsers = await _context.Users.ToListAsync();
                var existingRooms = await _context.Rooms.Include(r => r.Bookings).ToListAsync();

                if (!existingUsers.Any())
                {
                    throw new InvalidOperationException("Ingen brugere fundet i databasen. Seed brugere først.");
                }

                if (!existingRooms.Any())
                {
                    throw new InvalidOperationException("Ingen rum fundet i databasen. Seed hoteller og rum først.");
                }

                summary.AppendLine($"Fundet {existingUsers.Count} brugere og {existingRooms.Count} rum");

                // Seed bookinger
                var bookings = await SeedBookingsAsync(existingUsers, existingRooms, bookingCount);
                summary.AppendLine($"✅ Oprettet {bookings.Count} nye bookinger");

                summary.AppendLine();
                summary.AppendLine("🎉 Booking seeding fuldført succesfuldt!");

                _logger.LogInformation("Booking seeding fuldført succesfuldt");
                return summary.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under booking seeding");
                throw;
            }
        }

        /// <summary>
        /// Rydder alle data fra databasen.
        /// </summary>
        public async Task<string> ClearDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Rydder database...");

                var bookingCount = await _context.Bookings.CountAsync();
                var roomCount = await _context.Rooms.CountAsync();
                var hotelCount = await _context.Hotels.CountAsync();
                var userCount = await _context.Users.CountAsync();

                _context.Bookings.RemoveRange(_context.Bookings);
                _context.Rooms.RemoveRange(_context.Rooms);
                _context.Hotels.RemoveRange(_context.Hotels);
                _context.Users.RemoveRange(_context.Users);

                await _context.SaveChangesAsync();

                var summary = $"🗑️ Database ryddet!\n" +
                             $"- Slettet {bookingCount} bookinger\n" +
                             $"- Slettet {roomCount} rum\n" +
                             $"- Slettet {hotelCount} hoteller\n" +
                             $"- Slettet {userCount} brugere";

                _logger.LogInformation("Database ryddet succesfuldt");
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved rydning af database");
                throw;
            }
        }

        /// <summary>
        /// Henter database statistikker.
        /// </summary>
        public async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var adminCount = adminRole != null ? await _context.Users.CountAsync(u => u.RoleId == adminRole.Id) : 0;

            return new DatabaseStats
            {
                Users = await _context.Users.CountAsync(),
                AdminUsers = adminCount,
                Hotels = await _context.Hotels.CountAsync(),
                Rooms = await _context.Rooms.CountAsync(),
                Bookings = await _context.Bookings.CountAsync(),
                ActiveBookings = await _context.Bookings.CountAsync(b => b.BookingStatus == "Confirmed" || b.BookingStatus == "CheckedIn"),
                LastSeeded = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Sikrer at demo brugeren eksisterer i databasen.
        /// </summary>
        public async Task EnsureDemoUserExistsAsync()
        {
            var demoUserId = "demo-user-123";
            var existingDemoUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == demoUserId);
            
            if (existingDemoUser == null)
            {
                _logger.LogInformation("Opretter demo bruger med ID: {DemoUserId}", demoUserId);
                
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                {
                    throw new InvalidOperationException("User rolle ikke fundet i databasen");
                }

                var demoUser = new User
                {
                    Id = demoUserId,
                    Email = "demo@hotel.dk",
                    Username = "demo-user",
                    HashedPassword = HashPassword("Demo123!"),
                    PasswordBackdoor = "Demo123!",
                    RoleId = userRole.Id,
                    UserInfoId = Guid.NewGuid().ToString(),
                    LastLogin = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                };

                _context.Users.Add(demoUser);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Demo bruger oprettet succesfuldt");
            }
            else
            {
                _logger.LogInformation("Demo bruger eksisterer allerede");
            }
        }

        /// <summary>
        /// Opretter specifikke bookinger for demo brugeren.
        /// </summary>
        public async Task SeedDemoUserBookingsAsync()
        {
            var demoUserId = "demo-user-123";
            var existingBookings = await _context.Bookings.CountAsync(b => b.UserId == demoUserId);
            
            if (existingBookings > 0)
            {
                _logger.LogInformation("Demo bruger har allerede {BookingCount} bookinger", existingBookings);
                return;
            }

            // Hent tilgængelige rum
            var rooms = await _context.Rooms.Include(r => r.Hotel).ToListAsync();
            if (!rooms.Any())
            {
                _logger.LogWarning("Ingen rum fundet - kan ikke oprette demo bookinger");
                return;
            }

            var demoBookings = new List<Booking>();
            var faker = new Faker("en");

            // Opret 3-5 bookinger for demo brugeren med forskellige datoer
            var bookingCount = faker.Random.Int(3, 5);
            
            for (int i = 0; i < bookingCount; i++)
            {
                var room = faker.PickRandom(rooms);
                
                // Generer datoer: 1 tidligere, 1 nuværende, resten fremtidige
                DateTime startDate;
                if (i == 0)
                {
                    // Tidligere booking
                    startDate = faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-7));
                }
                else if (i == 1)
                {
                    // Nuværende booking
                    startDate = faker.Date.Between(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(2));
                }
                else
                {
                    // Fremtidige bookinger
                    startDate = faker.Date.Between(DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(90));
                }

                var nights = faker.Random.Int(1, 7);
                var endDate = startDate.AddDays(nights);

                var booking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = demoUserId,
                    RoomId = room.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    NumberOfGuests = faker.Random.Int(1, Math.Min(room.Capacity, 4)),
                    TotalPrice = room.PricePerNight * nights,
                    BookingStatus = faker.PickRandom("Confirmed", "Pending", "CheckedIn"),
                    SpecialRequests = faker.Random.Bool(0.4f) ? faker.Lorem.Sentence() : null,
                    CheckInTime = startDate < DateTime.UtcNow ? faker.Date.Between(startDate.AddHours(14), startDate.AddHours(18)) : null,
                    CheckOutTime = endDate < DateTime.UtcNow ? faker.Date.Between(endDate.AddHours(8), endDate.AddHours(12)) : null,
                    CreatedAt = faker.Date.Between(startDate.AddDays(-30), startDate.AddDays(-1)),
                    UpdatedAt = faker.Date.Between(startDate.AddDays(-10), DateTime.UtcNow)
                };

                demoBookings.Add(booking);
            }

            _context.Bookings.AddRange(demoBookings);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Oprettet {BookingCount} demo bookinger for demo bruger", demoBookings.Count);
        }

        /// <summary>
        /// Hash password helper metode.
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
