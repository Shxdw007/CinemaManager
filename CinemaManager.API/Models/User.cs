using System.ComponentModel.DataAnnotations;

namespace CinemaManager.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Храним только хэш!
        public string Role { get; set; } = string.Empty; // Admin, Manager, Cashier, Viewer
    }
}