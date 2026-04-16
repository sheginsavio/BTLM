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

        [Range(0, double.MaxValue, ErrorMessage = "Opening balance cannot be negative.")]
        [Display(Name = "Opening Balance")]
        [DataType(DataType.Currency)]
        public decimal InitialBalance { get; set; } = 0;
    }
}
