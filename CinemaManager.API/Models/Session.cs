using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaManager.API.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; } 

        public decimal TicketPrice { get; set; }
    }
}