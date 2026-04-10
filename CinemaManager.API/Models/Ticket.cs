using System.ComponentModel.DataAnnotations;

namespace CinemaManager.API.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int Row { get; set; }
        public int Seat { get; set; }

        [MaxLength(256)]
        public string UserEmail { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }
    }
}