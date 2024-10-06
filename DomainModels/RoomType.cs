using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    public class RoomType : Common
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } 

        [Required]
        public int Size { get; set; } 

        public string Description { get; set; }

        // Navigation property https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations
        public ICollection<Room> Rooms { get; set; }
    }
}
