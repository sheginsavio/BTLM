using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class AccountRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "PENDING";
        // Values: PENDING, APPROVED, REJECTED

        [Display(Name = "Requested On")]
        public DateTime RequestedOn { get; set; } = DateTime.Now;

        [Display(Name = "Reviewed On")]
        public DateTime? ReviewedOn { get; set; }

        [StringLength(200)]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        [StringLength(100)]
        [Display(Name = "Reviewed By")]
        public string? ReviewedBy { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
