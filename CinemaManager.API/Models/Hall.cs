using System.ComponentModel.DataAnnotations;

namespace CinemaManager.API.Models
{
    public class Hall
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 2D, 3D, VIP
        public int RowsCount { get; set; }
        public int SeatsPerRow { get; set; }
    }
}