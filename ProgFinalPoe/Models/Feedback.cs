using System.ComponentModel.DataAnnotations;

namespace ProgFinalPoe.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        [Required]
        public int ClaimId { get; set; } // Foreign key

        [Required, StringLength(50)]
        public string Role { get; set; } // Coordinator, Manager

        [Required, StringLength(1000)]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Claim Claim { get; set; }
    }
}
