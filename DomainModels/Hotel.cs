using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    // Hotel.cs
    public class Hotel : Common
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";

        public List<Room> Rooms { get; set; } = new(); // 1:n
    }

    // DTO for hotel creation / POST
    public class HotelPostDto
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
    }

    // DTO for hotel retrieval / GET
    public class HotelGetDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";

    }

    // DTO for hotel update / PUT
    public class HotelPutDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
    }
}