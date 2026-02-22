using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Mapping
{
    public class RoomMapping
    {
        /// <summary>
        /// Konverterer Room til RoomGetDto
        /// </summary>
        public static RoomGetDto ToRoomGetDto(Room room)
        {
            return new RoomGetDto
            {
                Id = room.Id,
                Number = room.Number,
                Capacity = room.Capacity,
                NumberOfBeds = room.NumberOfBeds,
                RoomType = room.RoomType,
                PricePerNight = room.PricePerNight,
                FloorNumber = room.FloorNumber,
                HasBalcony = room.HasBalcony,
                HasSeaView = room.HasSeaView,
                HasWifi = room.HasWifi,
                HasAirConditioning = room.HasAirConditioning,
                HasMinibar = room.HasMinibar,
                IsAccessible = room.IsAccessible,
                Description = room.Description,
                SquareMeters = room.SquareMeters,
                HotelId = room.HotelId,
                HotelName = room.Hotel?.Name ?? string.Empty,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }

        /// <summary>
        /// Konverterer liste af Room til liste af RoomGetDto
        /// </summary>
        public static List<RoomGetDto> ToRoomGetDtos(List<Room> rooms)
        {
            return rooms.Select(r => ToRoomGetDto(r)).ToList();
        }

        /// <summary>
        /// Konverterer RoomPostDto til Room entity
        /// </summary>
        public static Room ToRoomFromPostDto(RoomPostDto roomPostDto)
        {
            return new Room
            {
                Id = Guid.NewGuid().ToString(),
                Number = roomPostDto.Number,
                Capacity = roomPostDto.Capacity,
                NumberOfBeds = roomPostDto.NumberOfBeds,
                RoomType = roomPostDto.RoomType,
                PricePerNight = roomPostDto.PricePerNight,
                FloorNumber = roomPostDto.FloorNumber,
                HasBalcony = roomPostDto.HasBalcony,
                HasSeaView = roomPostDto.HasSeaView,
                HasWifi = roomPostDto.HasWifi,
                HasAirConditioning = roomPostDto.HasAirConditioning,
                HasMinibar = roomPostDto.HasMinibar,
                IsAccessible = roomPostDto.IsAccessible,
                Description = roomPostDto.Description,
                SquareMeters = roomPostDto.SquareMeters,
                HotelId = roomPostDto.HotelId,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                UpdatedAt = DateTime.UtcNow.AddHours(2)
            };
        }

        /// <summary>
        /// Opdaterer Room entity med data fra RoomPutDto
        /// </summary>
        public static void UpdateRoomFromPutDto(Room room, RoomPutDto roomPutDto)
        {
            room.Number = roomPutDto.Number;
            room.Capacity = roomPutDto.Capacity;
            room.NumberOfBeds = roomPutDto.NumberOfBeds;
            room.RoomType = roomPutDto.RoomType;
            room.PricePerNight = roomPutDto.PricePerNight;
            room.FloorNumber = roomPutDto.FloorNumber;
            room.HasBalcony = roomPutDto.HasBalcony;
            room.HasSeaView = roomPutDto.HasSeaView;
            room.HasWifi = roomPutDto.HasWifi;
            room.HasAirConditioning = roomPutDto.HasAirConditioning;
            room.HasMinibar = roomPutDto.HasMinibar;
            room.IsAccessible = roomPutDto.IsAccessible;
            room.Description = roomPutDto.Description;
            room.SquareMeters = roomPutDto.SquareMeters;
            room.HotelId = roomPutDto.HotelId;
            room.UpdatedAt = DateTime.UtcNow.AddHours(2);
        }

        /// <summary>
        /// Konverterer Room til RoomAvailabilityResultDto med availability information
        /// </summary>
        public static RoomAvailabilityResultDto ToRoomAvailabilityResultDto(Room room, bool isAvailable, int nights)
        {
            var features = new List<string>();
            if (room.HasBalcony) features.Add("Balkon");
            if (room.HasSeaView) features.Add("Havudsigt");
            if (room.HasWifi) features.Add("WiFi");
            if (room.HasAirConditioning) features.Add("Aircondition");
            if (room.HasMinibar) features.Add("Minibar");
            if (room.IsAccessible) features.Add("Handicapvenligt");

            return new RoomAvailabilityResultDto
            {
                Id = room.Id,
                Number = room.Number,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                NumberOfBeds = room.NumberOfBeds,
                PricePerNight = room.PricePerNight,
                TotalPrice = room.PricePerNight * nights,
                FloorNumber = room.FloorNumber,
                HasBalcony = room.HasBalcony,
                HasSeaView = room.HasSeaView,
                HasWifi = room.HasWifi,
                HasAirConditioning = room.HasAirConditioning,
                HasMinibar = room.HasMinibar,
                IsAccessible = room.IsAccessible,
                Description = room.Description,
                SquareMeters = room.SquareMeters,
                HotelName = room.Hotel?.Name ?? string.Empty,
                IsAvailable = isAvailable,
                Features = features
            };
        }
    }
}
