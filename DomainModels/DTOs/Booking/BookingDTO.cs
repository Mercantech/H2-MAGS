﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModels.DTOs.Room;  // Tilføj denne using statement

namespace DomainModels.DTOs.Booking
{
        public class GetBookingDTO
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string RoomId { get; set; }
            public DateTime CheckIn { get; set; }
            public DateTime CheckOut { get; set; }
            public RoomDetailsDTO RoomDetails { get; set; }  // Bruger den eksisterende RoomDetailsDTO
            public int TotalNights { get; set; }
            public decimal TotalPrice { get; set; }
            public string Status { get; set; }
        }

        public class CreateBookingDTO
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public string RoomId { get; set; }

            [Required]
            public DateTime CheckIn { get; set; }

            [Required]
            public DateTime CheckOut { get; set; }
        }

        public class UpdateBookingDTO
        {
            [Required]
            public string Id { get; set; }

            [Required]
            public string UserId { get; set; }

            [Required]
            public string RoomId { get; set; }

            [Required]
            public DateTime CheckIn { get; set; }

            [Required]
            public DateTime CheckOut { get; set; }
        }
}
