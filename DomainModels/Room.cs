using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    // Room.cs
    public class Room : Common
    {
        public string Number { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int NumberOfBeds { get; set; }
        public string RoomType { get; set; } = string.Empty; // Single, Double, Suite, Family, etc.
        public decimal PricePerNight { get; set; }
        public int FloorNumber { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasMinibar { get; set; }
        public bool IsAccessible { get; set; } // Handicapvenligt
        public string Description { get; set; } = string.Empty;
        public int SquareMeters { get; set; }

        public string HotelId { get; set; } = string.Empty;
        public Hotel? Hotel { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }

    // DTO for room creation / POST
    public class RoomPostDto
    {
        [Required(ErrorMessage = "Rum nummer er påkrævet")]
        [StringLength(10, ErrorMessage = "Rum nummer må maksimalt være 10 tegn")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kapacitet er påkrævet")]
        [Range(1, 10, ErrorMessage = "Kapacitet skal være mellem 1 og 10")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Antal senge er påkrævet")]
        [Range(1, 5, ErrorMessage = "Antal senge skal være mellem 1 og 5")]
        public int NumberOfBeds { get; set; }

        [Required(ErrorMessage = "Rum type er påkrævet")]
        [StringLength(50, ErrorMessage = "Rum type må maksimalt være 50 tegn")]
        public string RoomType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pris per nat er påkrævet")]
        [Range(0.01, 10000, ErrorMessage = "Pris skal være mellem 0.01 og 10000")]
        public decimal PricePerNight { get; set; }

        [Range(1, 50, ErrorMessage = "Etage skal være mellem 1 og 50")]
        public int FloorNumber { get; set; }

        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasMinibar { get; set; }
        public bool IsAccessible { get; set; }

        [StringLength(500, ErrorMessage = "Beskrivelse må maksimalt være 500 tegn")]
        public string Description { get; set; } = string.Empty;

        [Range(10, 200, ErrorMessage = "Kvadratmeter skal være mellem 10 og 200")]
        public int SquareMeters { get; set; }

        [Required(ErrorMessage = "Hotel ID er påkrævet")]
        public string HotelId { get; set; } = string.Empty;
    }

    // DTO for room retrieval / GET
    public class RoomGetDto
    {
        public string Id { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int NumberOfBeds { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int FloorNumber { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasMinibar { get; set; }
        public bool IsAccessible { get; set; }
        public string Description { get; set; } = string.Empty;
        public int SquareMeters { get; set; }
        public string HotelId { get; set; } = string.Empty;
        public string? HotelName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO for room update / PUT
    public class RoomPutDto
    {
        [Required(ErrorMessage = "Rum ID er påkrævet")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rum nummer er påkrævet")]
        [StringLength(10, ErrorMessage = "Rum nummer må maksimalt være 10 tegn")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kapacitet er påkrævet")]
        [Range(1, 10, ErrorMessage = "Kapacitet skal være mellem 1 og 10")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Antal senge er påkrævet")]
        [Range(1, 5, ErrorMessage = "Antal senge skal være mellem 1 og 5")]
        public int NumberOfBeds { get; set; }

        [Required(ErrorMessage = "Rum type er påkrævet")]
        [StringLength(50, ErrorMessage = "Rum type må maksimalt være 50 tegn")]
        public string RoomType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pris per nat er påkrævet")]
        [Range(0.01, 10000, ErrorMessage = "Pris skal være mellem 0.01 og 10000")]
        public decimal PricePerNight { get; set; }

        [Range(1, 50, ErrorMessage = "Etage skal være mellem 1 og 50")]
        public int FloorNumber { get; set; }

        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasMinibar { get; set; }
        public bool IsAccessible { get; set; }

        [StringLength(500, ErrorMessage = "Beskrivelse må maksimalt være 500 tegn")]
        public string Description { get; set; } = string.Empty;

        [Range(10, 200, ErrorMessage = "Kvadratmeter skal være mellem 10 og 200")]
        public int SquareMeters { get; set; }

        [Required(ErrorMessage = "Hotel ID er påkrævet")]
        public string HotelId { get; set; } = string.Empty;
    }

    // DTO for availability search queries
    public class RoomAvailabilityQueryDto
    {
        [Required(ErrorMessage = "Hotel ID er påkrævet")]
        public string HotelId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Check-in dato er påkrævet")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out dato er påkrævet")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Antal gæster er påkrævet")]
        [Range(1, 10, ErrorMessage = "Antal gæster skal være mellem 1 og 10")]
        public int NumberOfGuests { get; set; }

        [Range(1, 5, ErrorMessage = "Minimum antal senge skal være mellem 1 og 5")]
        public int? MinimumBeds { get; set; }

        [StringLength(50, ErrorMessage = "Rum type må maksimalt være 50 tegn")]
        public string? RoomType { get; set; }

        [Range(0.01, 10000, ErrorMessage = "Maximum pris skal være mellem 0.01 og 10000")]
        public decimal? MaxPricePerNight { get; set; }

        public bool? RequireBalcony { get; set; }
        public bool? RequireSeaView { get; set; }
        public bool? RequireWifi { get; set; }
        public bool? RequireAirConditioning { get; set; }
        public bool? RequireMinibar { get; set; }
        public bool? RequireAccessible { get; set; }

        [Range(1, 50, ErrorMessage = "Etage skal være mellem 1 og 50")]
        public int? PreferredFloor { get; set; }

        [Range(10, 200, ErrorMessage = "Minimum kvadratmeter skal være mellem 10 og 200")]
        public int? MinimumSquareMeters { get; set; }
    }

    // DTO for availability search results
    public class RoomAvailabilityResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int NumberOfBeds { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public int FloorNumber { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasMinibar { get; set; }
        public bool IsAccessible { get; set; }
        public string Description { get; set; } = string.Empty;
        public int SquareMeters { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public List<string> Features { get; set; } = new();
    }
}
