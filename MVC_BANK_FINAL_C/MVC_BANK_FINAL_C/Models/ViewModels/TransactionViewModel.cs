using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class TransactionViewModel
    {
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Account is required")]
        [Display(Name = "Account ID")]
        public int AccountId { get; set; }

        [Display(Name = "To Account ID")]
        public int? ToAccountId { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // ── New properties ────────────────────────────────────────────

        /// <summary>"own" = customer's own accounts; "external" = another customer's account</summary>
        public string? TransferType { get; set; }

        /// <summary>Username from session — written into AuditLog.PerformedBy</summary>
        public string? PerformedBy { get; set; }

        /// <summary>Pre-populated dropdown for the Customer's own accounts (From field)</summary>
        public List<SelectListItem> FromAccountList { get; set; } = new();

        /// <summary>Pre-populated dropdown for the Customer's own accounts (To field — own-to-own)</summary>
        public List<SelectListItem> ToAccountList { get; set; } = new();
    }
}
