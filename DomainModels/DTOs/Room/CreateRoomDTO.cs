using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModels.DTOs.RoomType;

namespace DomainModels.DTOs.Room
{
        public class CreateRoomDTO
        {
            public string RoomNumber { get; set; }
            public string RoomTypeId { get; set; }
            public decimal PricePerNight { get; set; }
        }

        public class GetRoomDTO
        {
            public string Id { get; set; }
            public string RoomNumber { get; set; }
            public string RoomTypeId { get; set; }
            public decimal PricePerNight { get; set; }
        }
        public class RoomDetailsDTO
        {
            public string Id { get; set; }
            public string RoomNumber { get; set; }
            public string RoomTypeId { get; set; }
            public decimal PricePerNight { get; set; }
            public GetRoomTypeDTO RoomType { get; set; }
        }
}
