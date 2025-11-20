using System.ComponentModel.DataAnnotations;

namespace ProgFinalPoe.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; } // Lecturer, Coordinator, Manager, HR
        public int? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public decimal HourlyRate { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}