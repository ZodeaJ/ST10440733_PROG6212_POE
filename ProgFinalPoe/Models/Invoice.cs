using System.ComponentModel.DataAnnotations.Schema;

namespace ProgFinalPoe.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public int ClaimId { get; set; }

        public int LecturerId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; } = false;

        public Lecturer Lecturer { get; set; }
        public Claim Claim { get; set; }
    }
}