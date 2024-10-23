using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.DTOs.RoomType
{
    public class CreateRoomTypeDTO
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Description { get; set; }
    }
    public class GetRoomTypeDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string Description { get; set; }
    }
}
