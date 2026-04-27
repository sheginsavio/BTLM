using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly BankingDbContext _context;

        public TransactionService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> DepositFunds(TransactionViewModel vm)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(vm.AccountId);
                if (account == null) return null;

                // Check single transaction limit
                if (vm.Amount > 200000)
                    return null;

                // Check account balance cap (10 Crores)
                if (account.Balance + vm.Amount > 10000000)
                    return null;

                account.Balance += vm.Amount;

                var transaction = new Transaction
                {
                    AccountId       = vm.AccountId,
                    TransactionType = TransactionType.DEPOSIT,
                    Amount          = vm.Amount,
                    TransactionDate = DateTime.Now
                };
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    TransactionId   = transaction.TransactionId,
                    LogDate         = DateTime.Now,
                    ActionPerformed = $"Deposited {vm.Amount:C} to Account {vm.AccountId}",
                    PerformedBy     = vm.PerformedBy ?? "System"
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return transaction;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Transaction?> WithdrawFunds(TransactionViewModel vm)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(vm.AccountId);
                if (account == null) return null;

                // Check single transaction limit
                if (vm.Amount > 200000)
                    return null;

                if (account.Balance < vm.Amount) return null;

                account.Balance -= vm.Amount;

                var transaction = new Transaction
                {
                    AccountId       = vm.AccountId,
                    TransactionType = TransactionType.WITHDRAWAL,
                    Amount          = vm.Amount,
                    TransactionDate = DateTime.Now
                };
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    TransactionId   = transaction.TransactionId,
                    LogDate         = DateTime.Now,
                    ActionPerformed = $"Withdrew {vm.Amount:C} from Account {vm.AccountId}",
                    PerformedBy     = vm.PerformedBy ?? "System"
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return transaction;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> TransferFunds(int fromAccountId, int toAccountId, decimal amount, string performedBy = "System")
        {
            try
            {
                // Check single transaction limit
                if (amount > 200000)
                    return false;

                var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
                var toAccount   = await _context.Accounts.FindAsync(toAccountId);

                if (fromAccount == null || toAccount == null) return false;
                if (fromAccount.Balance < amount) return false;

                fromAccount.Balance -= amount;
                toAccount.Balance   += amount;

                var transaction = new Transaction
                {
                    AccountId       = fromAccountId,
                    TransactionType = TransactionType.TRANSFER,
                    Amount          = amount,
                    TransactionDate = DateTime.Now
                };
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    TransactionId   = transaction.TransactionId,
                    LogDate         = DateTime.Now,
                    ActionPerformed = $"Transferred {amount:C} from Account {fromAccountId} to Account {toAccountId}",
                    PerformedBy     = performedBy
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            return await _context.Transactions
                .Include(t => t.Account)
                    .ThenInclude(a => a!.Customer)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
