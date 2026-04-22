using NUnit.Framework;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Implementations;
using Tests.Helpers;

namespace Tests.Services
{
    [TestFixture]
    public class LoanServiceTests
    {
        private LoanService _service;
        private BankingDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = DbContextMockHelper.CreateInMemoryContext("LoanDb");
            _service = new LoanService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private async Task<(Customer customer, Account account)> CreateTestCustomerWithAccount()
        {
            var customer = new Customer { Name = "Test Customer", Email = "test@test.com" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var account = new Account
            {
                CustomerId  = customer.CustomerId,
                AccountType = AccountType.SAVINGS,
                Balance     = 5000m
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return (customer, account);
        }

        private LoanViewModel CreateLoanViewModel(int customerId, int accountId) =>
            new LoanViewModel
            {
                CustomerId      = customerId,
                LoanAmount      = 100000m,
                InterestRate    = 7m,
                LoanType        = "Home",
                Tenure          = 2,
                MonthlyEMI      = 4833.33m,
                CreditAccountId = accountId
            };

        // APPLY TESTS
        [Test]
        public async Task ApplyLoan_ValidViewModel_CreatesLoanWithAppliedStatus()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            var vm = CreateLoanViewModel(customer.CustomerId, account.AccountId);

            var result = await _service.ApplyLoan(vm);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.LoanStatus, Is.EqualTo(LoanStatus.APPLIED));
            Assert.That(result.LoanAmount, Is.EqualTo(100000m));
        }

        [Test]
        public async Task ApplyLoan_ValidViewModel_StoresCorrectInterestRate()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            var vm = CreateLoanViewModel(customer.CustomerId, account.AccountId);

            var result = await _service.ApplyLoan(vm);

            Assert.That(result.InterestRate, Is.EqualTo(7m));
        }

        [Test]
        public async Task ApplyLoan_ValidViewModel_StoresCorrectEMI()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            var vm = CreateLoanViewModel(customer.CustomerId, account.AccountId);

            var result = await _service.ApplyLoan(vm);

            Assert.That(result.MonthlyEMI, Is.EqualTo(4833.33m));
        }

        // APPROVE TESTS
        [Test]
        public async Task ApproveLoan_ValidDecisionApprove_ChangesStatusToApproved()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            var loan = await _service.ApplyLoan(
                CreateLoanViewModel(customer.CustomerId, account.AccountId));

            var result = await _service.ApproveLoan(loan.LoanId, "APPROVE");

            Assert.That(result!.LoanStatus, Is.EqualTo(LoanStatus.APPROVED));
        }

        [Test]
        public async Task ApproveLoan_ValidDecisionApprove_CreditesAccountBalance()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            decimal initialBalance = account.Balance;
            var loan = await _service.ApplyLoan(
                CreateLoanViewModel(customer.CustomerId, account.AccountId));

            await _service.ApproveLoan(loan.LoanId, "APPROVE");

            var updatedAccount = await _context.Accounts.FindAsync(account.AccountId);
            Assert.That(updatedAccount!.Balance,
                Is.EqualTo(initialBalance + loan.LoanAmount));
        }

        [Test]
        public async Task ApproveLoan_ValidDecisionReject_ChangesStatusToRejected()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            var loan = await _service.ApplyLoan(
                CreateLoanViewModel(customer.CustomerId, account.AccountId));

            var result = await _service.ApproveLoan(loan.LoanId, "REJECT");

            Assert.That(result!.LoanStatus, Is.EqualTo(LoanStatus.REJECTED));
        }

        [Test]
        public async Task ApproveLoan_ValidDecisionReject_DoesNotCreditAccount()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            decimal initialBalance = account.Balance;
            var loan = await _service.ApplyLoan(
                CreateLoanViewModel(customer.CustomerId, account.AccountId));

            await _service.ApproveLoan(loan.LoanId, "REJECT");

            var updatedAccount = await _context.Accounts.FindAsync(account.AccountId);
            Assert.That(updatedAccount!.Balance, Is.EqualTo(initialBalance));
        }

        [Test]
        public async Task ApproveLoan_InvalidLoanId_ReturnsNull()
        {
            var result = await _service.ApproveLoan(9999, "APPROVE");

            Assert.That(result, Is.Null);
        }

        // GET TESTS
        [Test]
        public async Task GetLoanDetails_ExistingLoan_ReturnsCorrectLoan()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            var loan = await _service.ApplyLoan(
                CreateLoanViewModel(customer.CustomerId, account.AccountId));

            var result = await _service.GetLoanDetails(loan.LoanId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.LoanId, Is.EqualTo(loan.LoanId));
        }

        [Test]
        public async Task GetLoanDetails_NonExistingLoan_ReturnsNull()
        {
            var result = await _service.GetLoanDetails(9999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllLoans_ReturnsAllLoans()
        {
            var (customer, account) = await CreateTestCustomerWithAccount();
            await _service.ApplyLoan(CreateLoanViewModel(customer.CustomerId, account.AccountId));
            await _service.ApplyLoan(CreateLoanViewModel(customer.CustomerId, account.AccountId));

            var result = await _service.GetAllLoans();

            Assert.That(result.Count(), Is.EqualTo(2));
        }
    }
}
