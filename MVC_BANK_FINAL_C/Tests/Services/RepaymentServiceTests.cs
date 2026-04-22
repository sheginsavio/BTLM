using NUnit.Framework;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Implementations;
using Tests.Helpers;

namespace Tests.Services
{
    [TestFixture]
    public class RepaymentServiceTests
    {
        private RepaymentService _service;
        private BankingDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = DbContextMockHelper.CreateInMemoryContext("RepaymentDb");
            _service = new RepaymentService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private async Task<(Customer customer, Account account, Loan loan)>
            CreateApprovedLoan(decimal loanAmount = 100000m,
                               decimal interestRate = 5m,
                               int tenure = 2)
        {
            var customer = new Customer { Name = "Test", Email = "test@test.com" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var account = new Account
            {
                CustomerId  = customer.CustomerId,
                AccountType = AccountType.SAVINGS,
                Balance     = 50000m
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var loan = new Loan
            {
                CustomerId      = customer.CustomerId,
                LoanAmount      = loanAmount,
                InterestRate    = interestRate,
                Tenure          = tenure,
                LoanType        = "Personal",
                MonthlyEMI      = 4583.33m,
                LoanStatus      = LoanStatus.APPROVED,
                CreditAccountId = account.AccountId
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return (customer, account, loan);
        }

        // INTEREST CALCULATION TESTS
        [Test]
        public async Task CalculateInterest_CorrectFormula_ReturnsExpectedValue()
        {
            // P=100000, R=5, T=2 → SI = 100000 * 5 * 2 / 100 = 10000
            var (_, _, loan) = await CreateApprovedLoan(100000m, 5m, 2);

            var result = await _service.CalculateInterest(loan.LoanId);

            Assert.That(result, Is.EqualTo(10000m));
        }

        [Test]
        public async Task CalculateInterest_InvalidLoanId_ReturnsZero()
        {
            var result = await _service.CalculateInterest(9999);

            Assert.That(result, Is.EqualTo(0m));
        }

        // RECORD REPAYMENT TESTS (Admin/LoanOfficer flow)
        [Test]
        public async Task RecordRepayment_FirstRepayment_UsesTotalRepayableAsBase()
        {
            // P=100000, R=5, T=2 → Total = 110000
            var (_, _, loan) = await CreateApprovedLoan(100000m, 5m, 2);
            var vm = new RepaymentViewModel
            {
                LoanId     = loan.LoanId,
                AmountPaid = 10000m
            };

            var result = await _service.RecordRepayment(vm);

            // BalanceRemaining should be 110000 - 10000 = 100000
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.BalanceRemaining, Is.EqualTo(100000m));
        }

        [Test]
        public async Task RecordRepayment_AmountExceedsBalance_ReturnsNull()
        {
            // Total repayable = 110000
            var (_, _, loan) = await CreateApprovedLoan(100000m, 5m, 2);
            var vm = new RepaymentViewModel
            {
                LoanId     = loan.LoanId,
                AmountPaid = 200000m  // exceeds total
            };

            var result = await _service.RecordRepayment(vm);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RecordRepayment_InvalidLoanId_ReturnsNull()
        {
            var vm = new RepaymentViewModel
            {
                LoanId     = 9999,
                AmountPaid = 1000m
            };

            var result = await _service.RecordRepayment(vm);

            Assert.That(result, Is.Null);
        }

        // CUSTOMER REPAYMENT TESTS
        [Test]
        public async Task RecordCustomerRepayment_ValidData_DeductsFromAccountBalance()
        {
            var (customer, account, loan) = await CreateApprovedLoan(100000m, 5m, 2);
            decimal initialBalance = account.Balance;
            var vm = new RepaymentViewModel
            {
                LoanId       = loan.LoanId,
                AmountPaid   = 5000m,
                AccountId    = account.AccountId,
                CustomerName = customer.Name
            };

            var result = await _service.RecordCustomerRepayment(vm);

            var updatedAccount = await _context.Accounts.FindAsync(account.AccountId);
            Assert.That(result, Is.Not.Null);
            Assert.That(updatedAccount!.Balance, Is.EqualTo(initialBalance - 5000m));
        }

        [Test]
        public async Task RecordCustomerRepayment_InsufficientAccountBalance_ReturnsNull()
        {
            // Account balance is 50000 but trying to pay 60000
            var (customer, account, loan) = await CreateApprovedLoan(100000m, 5m, 2);
            var vm = new RepaymentViewModel
            {
                LoanId       = loan.LoanId,
                AmountPaid   = 60000m,
                AccountId    = account.AccountId,
                CustomerName = customer.Name
            };

            var result = await _service.RecordCustomerRepayment(vm);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RecordCustomerRepayment_AmountExceedsLoanBalance_ReturnsNull()
        {
            // Total repayable = 110000, trying to pay 120000
            var (customer, account, loan) = await CreateApprovedLoan(100000m, 5m, 2);
            var vm = new RepaymentViewModel
            {
                LoanId       = loan.LoanId,
                AmountPaid   = 120000m,
                AccountId    = account.AccountId,
                CustomerName = customer.Name
            };

            var result = await _service.RecordCustomerRepayment(vm);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RecordCustomerRepayment_ReducesLoanBalanceCorrectly()
        {
            // Total repayable = 110000, paying 10000 → remaining = 100000
            var (customer, account, loan) = await CreateApprovedLoan(100000m, 5m, 2);
            var vm = new RepaymentViewModel
            {
                LoanId       = loan.LoanId,
                AmountPaid   = 10000m,
                AccountId    = account.AccountId,
                CustomerName = customer.Name
            };

            var result = await _service.RecordCustomerRepayment(vm);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.BalanceRemaining, Is.EqualTo(100000m));
        }

        [Test]
        public async Task GetRepaymentHistory_ReturnsAllRepayments()
        {
            var (customer, account, loan) = await CreateApprovedLoan();

            await _service.RecordRepayment(new RepaymentViewModel
                { LoanId = loan.LoanId, AmountPaid = 5000m });
            await _service.RecordRepayment(new RepaymentViewModel
                { LoanId = loan.LoanId, AmountPaid = 5000m });

            var history = await _service.GetRepaymentHistory(loan.LoanId);

            Assert.That(history.Count(), Is.EqualTo(2));
        }
    }
}
