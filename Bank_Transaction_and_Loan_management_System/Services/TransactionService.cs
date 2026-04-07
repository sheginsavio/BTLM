using Bank_Transaction_and_Loan_management_System.Interfaces;
using Bank_Transaction_and_Loan_management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank_Transaction_and_Loan_management_System.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly BankingDbContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(BankingDbContext context, ILogger<TransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private AuditLog CreateAuditLog(int transactionId, string action, int userId)
        {
            return new AuditLog
            {
                TransactionId = transactionId,
                LogDate = DateTime.UtcNow,
                ActionPerformed = action,
                PerformedBy = userId.ToString()
            };
        }

        private async Task<bool> UserOwnsAccountAsync(int accountId, int userId)
        {
            var account = await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
                return false;

            // Verify that the account belongs to a customer owned by this user
            return account.Customer?.UserId == userId;
        }

        private TransactionDto MapToTransactionDto(BankTransaction transaction)
        {
            return new TransactionDto
            {
                TransactionId = transaction.TransactionId,
                AccountId = transaction.AccountId,
                TransactionType = transaction.TransactionType.ToString(),
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate
            };
        }

        public async Task<TransactionDto> DepositAsync(int accountId, decimal amount, int userId)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            if (!await UserOwnsAccountAsync(accountId, userId))
                throw new UnauthorizedAccessException("You don't have access to this account.");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                throw new InvalidOperationException("Account not found.");

            if (!account.IsActive)
                throw new InvalidOperationException("Account is inactive. Operations not allowed.");

            account.Balance += amount;
            var transaction = new BankTransaction
            {
                AccountId = accountId,
                TransactionType = TransactionType.DEPOSIT,
                Amount = amount,
                TransactionDate = DateTime.UtcNow
            };

            _context.BankTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var auditLog = CreateAuditLog(transaction.TransactionId, "Deposit", userId);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deposit successful: User {UserId}, Account {AccountId}, Amount {Amount}", userId, accountId, amount);
            return MapToTransactionDto(transaction);
        }

        public async Task<TransactionDto> WithdrawAsync(int accountId, decimal amount, int userId)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            if (!await UserOwnsAccountAsync(accountId, userId))
                throw new UnauthorizedAccessException("You don't have access to this account.");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                throw new InvalidOperationException("Account not found.");

            if (!account.IsActive)
                throw new InvalidOperationException("Account is inactive. Operations not allowed.");

            if (account.Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            account.Balance -= amount;
            var transaction = new BankTransaction
            {
                AccountId = accountId,
                TransactionType = TransactionType.WITHDRAWAL,
                Amount = amount,
                TransactionDate = DateTime.UtcNow
            };

            _context.BankTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var auditLog = CreateAuditLog(transaction.TransactionId, "Withdraw", userId);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Withdrawal successful: User {UserId}, Account {AccountId}, Amount {Amount}", userId, accountId, amount);
            return MapToTransactionDto(transaction);
        }

        public async Task<TransactionDto> TransferAsync(int fromAccountId, int toAccountId, decimal amount, int userId)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            if (fromAccountId == toAccountId)
                throw new ArgumentException("Cannot transfer to the same account.");

            if (!await UserOwnsAccountAsync(fromAccountId, userId))
                throw new UnauthorizedAccessException("You don't have access to this account.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
                var toAccount = await _context.Accounts.FindAsync(toAccountId);

                if (fromAccount == null || toAccount == null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Transfer failed: One or both accounts not found. From {FromAccountId}, To {ToAccountId}", fromAccountId, toAccountId);
                    throw new InvalidOperationException("One or both accounts not found.");
                }

                if (!fromAccount.IsActive || !toAccount.IsActive)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Transfer failed: One or both accounts are inactive. From {FromAccountId}, To {ToAccountId}", fromAccountId, toAccountId);
                    throw new InvalidOperationException("One or both accounts are inactive. Operations not allowed.");
                }

                if (fromAccount.Balance < amount)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Transfer failed: Insufficient balance. User {UserId}, Account {FromAccountId}", userId, fromAccountId);
                    throw new InvalidOperationException("Insufficient balance.");
                }

                fromAccount.Balance -= amount;
                toAccount.Balance += amount;

                var fromTransaction = new BankTransaction
                {
                    AccountId = fromAccountId,
                    TransactionType = TransactionType.TRANSFER,
                    Amount = amount,
                    TransactionDate = DateTime.UtcNow
                };

                var toTransaction = new BankTransaction
                {
                    AccountId = toAccountId,
                    TransactionType = TransactionType.TRANSFER,
                    Amount = amount,
                    TransactionDate = DateTime.UtcNow
                };

                _context.BankTransactions.Add(fromTransaction);
                _context.BankTransactions.Add(toTransaction);
                await _context.SaveChangesAsync();

                var fromAuditLog = CreateAuditLog(fromTransaction.TransactionId, "Transfer Out", userId);
                var toAuditLog = CreateAuditLog(toTransaction.TransactionId, "Transfer In", userId);

                _context.AuditLogs.Add(fromAuditLog);
                _context.AuditLogs.Add(toAuditLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Transfer successful: User {UserId}, From {FromAccountId}, To {ToAccountId}, Amount {Amount}", userId, fromAccountId, toAccountId, amount);
                return MapToTransactionDto(fromTransaction);
            }
        }

        public async Task<List<TransactionDto>> GetUserTransactionsAsync(int userId)
        {
            var transactions = await _context.BankTransactions
                .Include(bt => bt.Account)
                .ThenInclude(a => a.Customer)
                .Where(bt => bt.Account.Customer.UserId == userId)
                .Select(bt => new TransactionDto
                {
                    TransactionId = bt.TransactionId,
                    AccountId = bt.AccountId,
                    TransactionType = bt.TransactionType.ToString(),
                    Amount = bt.Amount,
                    TransactionDate = bt.TransactionDate
                })
                .ToListAsync();

            return transactions;
        }
    }
}
