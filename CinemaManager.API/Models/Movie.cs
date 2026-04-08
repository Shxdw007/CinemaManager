using System.ComponentModel.DataAnnotations;

namespace CinemaManager.API.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Duration { get; set; } // В минутах
        public string AgeRating { get; set; } = string.Empty; // 0+, 6+, 12+, 16+, 18+
        public string Director { get; set; } = string.Empty;

        // Будем хранить изображение в Base64 или путь к файлу
        public string PosterData { get; set; } = string.Empty;
    }
}