using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction?> DepositFunds(TransactionViewModel vm);
        Task<Transaction?> WithdrawFunds(TransactionViewModel vm);

        /// <summary>performedBy defaults to "System" for backward compatibility.</summary>
        Task<bool> TransferFunds(int fromAccountId, int toAccountId, decimal amount, string performedBy = "System");

        Task<IEnumerable<Transaction>> GetAllTransactions();
    }
}
