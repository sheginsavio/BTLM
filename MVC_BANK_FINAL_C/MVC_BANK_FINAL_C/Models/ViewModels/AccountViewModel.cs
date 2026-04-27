using System.ComponentModel.DataAnnotations;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class AccountViewModel
    {
        public int CustomerId { get; set; }

        /// <summary>Read-only display — not bound from form POST.</summary>
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Account type is required")]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Range(0, 100000, ErrorMessage =
            "Opening balance must be between ₹0 and ₹1,00,000.")]
        [Display(Name = "Opening Balance (Max ₹1,00,000)")]
        [DataType(DataType.Currency)]
        public decimal InitialBalance { get; set; } = 0;
    }
}
