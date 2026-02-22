using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Mapping
{
    public class HotelMapping
    {
        public static HotelGetDto ToHotelGetDto(Hotel hotel)
        {
            return new HotelGetDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address               
            };
        }

        public static List<HotelGetDto> ToHotelGetDtos(List<Hotel> hotels)
        {
            return hotels.Select(h => ToHotelGetDto(h)).ToList();
        }

        public static Hotel ToHotelFromDto(HotelPostDto hotelPostDto)
        {
            return new Hotel
            {
                Id = Guid.NewGuid().ToString(),
                Name = hotelPostDto.Name,
                Address = hotelPostDto.Address,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                UpdatedAt = DateTime.UtcNow.AddHours(2)
            };
        }

        public static void UpdateHotelFromDto(Hotel hotel, HotelPutDto hotelPutDto)
        {
            hotel.Name = hotelPutDto.Name;
            hotel.Address = hotelPutDto.Address;
            hotel.UpdatedAt = DateTime.UtcNow.AddHours(2);
        }
    }
}
