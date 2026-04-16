using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class RepaymentViewModel
    {
        public int RepaymentId { get; set; }

        [Required(ErrorMessage = "Loan is required")]
        [Display(Name = "Loan ID")]
        public int LoanId { get; set; }

        [Required]
        [Display(Name = "Repayment Date")]
        [DataType(DataType.Date)]
        public DateTime RepaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Amount paid is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Balance Remaining")]
        public decimal? BalanceRemaining { get; set; }

        // ── Customer repayment properties ─────────────────────────────────────

        /// <summary>Account to deduct repayment amount from (Customer only).</summary>
        [Display(Name = "Pay From Account")]
        public int? AccountId { get; set; }

        /// <summary>Pre-populated dropdown of the customer's own accounts.</summary>
        public List<SelectListItem> AccountList { get; set; } = new();

        /// <summary>Display name of the customer — written to session/audit context.</summary>
        public string? CustomerName { get; set; }
    }
}
