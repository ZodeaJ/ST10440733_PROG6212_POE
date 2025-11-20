using System.ComponentModel.DataAnnotations;

namespace ProgFinalPoe.Models
{
    public class Lecturer
    {
        public int LecturerId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress, StringLength(180)]
        public string Email { get; set; }

        [Required, Phone, StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required, StringLength(100)]
        public string Department { get; set; }

        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
