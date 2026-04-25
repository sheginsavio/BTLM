using System.ComponentModel.DataAnnotations;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class AccountRequestViewModel
    {
        public int RequestId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Please select an account type")]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        public string Status { get; set; } = "PENDING";

        [Display(Name = "Rejection Reason")]
        [StringLength(200)]
        public string? RejectionReason { get; set; }
    }
}
