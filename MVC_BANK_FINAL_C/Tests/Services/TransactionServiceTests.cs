using NUnit.Framework;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Implementations;
using Tests.Helpers;

namespace Tests.Services
{
    [TestFixture]
    public class TransactionServiceTests
    {
        private TransactionService _service;
        private BankingDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = DbContextMockHelper.CreateInMemoryContext("TransactionDb");
            _service = new TransactionService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private async Task<Account> CreateTestAccount(decimal balance = 10000m)
        {
            var customer = new Customer { Name = "Test", Email = "test@test.com" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var account = new Account
            {
                CustomerId  = customer.CustomerId,
                AccountType = AccountType.SAVINGS,
                Balance     = balance
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        // DEPOSIT TESTS
        [Test]
        public async Task DepositFunds_ValidAccount_IncreasesBalance()
        {
            var account = await CreateTestAccount(5000m);
            var vm = new TransactionViewModel
            {
                AccountId       = account.AccountId,
                Amount          = 2000m,
                TransactionType = TransactionType.DEPOSIT,
                PerformedBy     = "teller1"
            };

            var result = await _service.DepositFunds(vm);

            var updated = await _context.Accounts.FindAsync(account.AccountId);
            Assert.That(result, Is.Not.Null);
            Assert.That(updated!.Balance, Is.EqualTo(7000m));
        }

        [Test]
        public async Task DepositFunds_InvalidAccount_ReturnsNull()
        {
            var vm = new TransactionViewModel
            {
                AccountId       = 9999,
                Amount          = 1000m,
                TransactionType = TransactionType.DEPOSIT,
                PerformedBy     = "teller1"
            };

            var result = await _service.DepositFunds(vm);

            Assert.That(result, Is.Null);
        }

        // WITHDRAW TESTS
        [Test]
        public async Task WithdrawFunds_SufficientBalance_DecreasesBalance()
        {
            var account = await CreateTestAccount(10000m);
            var vm = new TransactionViewModel
            {
                AccountId       = account.AccountId,
                Amount          = 3000m,
                TransactionType = TransactionType.WITHDRAWAL,
                PerformedBy     = "teller1"
            };

            var result = await _service.WithdrawFunds(vm);

            var updated = await _context.Accounts.FindAsync(account.AccountId);
            Assert.That(result, Is.Not.Null);
            Assert.That(updated!.Balance, Is.EqualTo(7000m));
        }

        [Test]
        public async Task WithdrawFunds_InsufficientBalance_ReturnsNull()
        {
            var account = await CreateTestAccount(1000m);
            var vm = new TransactionViewModel
            {
                AccountId       = account.AccountId,
                Amount          = 5000m,
                TransactionType = TransactionType.WITHDRAWAL,
                PerformedBy     = "teller1"
            };

            var result = await _service.WithdrawFunds(vm);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task WithdrawFunds_InvalidAccount_ReturnsNull()
        {
            var vm = new TransactionViewModel
            {
                AccountId       = 9999,
                Amount          = 100m,
                TransactionType = TransactionType.WITHDRAWAL,
                PerformedBy     = "teller1"
            };

            var result = await _service.WithdrawFunds(vm);

            Assert.That(result, Is.Null);
        }

        // TRANSFER TESTS
        [Test]
        public async Task TransferFunds_ValidAccounts_MovesAmountCorrectly()
        {
            var fromAccount = await CreateTestAccount(10000m);
            var toAccount   = await CreateTestAccount(2000m);

            var result = await _service.TransferFunds(
                fromAccount.AccountId, toAccount.AccountId, 3000m);

            var updatedFrom = await _context.Accounts.FindAsync(fromAccount.AccountId);
            var updatedTo   = await _context.Accounts.FindAsync(toAccount.AccountId);

            Assert.That(result, Is.True);
            Assert.That(updatedFrom!.Balance, Is.EqualTo(7000m));
            Assert.That(updatedTo!.Balance,   Is.EqualTo(5000m));
        }

        [Test]
        public async Task TransferFunds_InsufficientBalance_ReturnsFalse()
        {
            var fromAccount = await CreateTestAccount(500m);
            var toAccount   = await CreateTestAccount(1000m);

            var result = await _service.TransferFunds(
                fromAccount.AccountId, toAccount.AccountId, 2000m);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TransferFunds_InvalidFromAccount_ReturnsFalse()
        {
            var toAccount = await CreateTestAccount(1000m);

            var result = await _service.TransferFunds(9999, toAccount.AccountId, 500m);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TransferFunds_InvalidToAccount_ReturnsFalse()
        {
            var fromAccount = await CreateTestAccount(5000m);

            var result = await _service.TransferFunds(fromAccount.AccountId, 9999, 500m);

            Assert.That(result, Is.False);
        }
    }
}
