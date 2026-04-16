using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [Display(Name = "Transaction")]
        public int TransactionId { get; set; }

        [Required]
        [Display(Name = "Log Date")]
        public DateTime LogDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(500, ErrorMessage = "Action performed cannot exceed 500 characters")]
        [Display(Name = "Action Performed")]
        public string ActionPerformed { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Performed by cannot exceed 100 characters")]
        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; } = "System";

        // Navigation properties
        [ForeignKey("TransactionId")]
        public Transaction? Transaction { get; set; }
    }
}
