using System.ComponentModel.DataAnnotations;

namespace CinemaManager.API.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public string Status { get; set; } = string.Empty; // "Paid", "Booked"
    }
}