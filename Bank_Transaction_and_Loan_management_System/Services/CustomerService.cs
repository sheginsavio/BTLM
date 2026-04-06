using Bank_Transaction_and_Loan_management_System.Interfaces;
using Bank_Transaction_and_Loan_management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank_Transaction_and_Loan_management_System.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly BankingDbContext _context;

        public CustomerService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }
    }
}
