using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly BankingDbContext _context;

        public CustomerService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> CreateAccount(CustomerViewModel vm)
        {
            var customer = new Customer
            {
                Name = vm.Name,
                Email = vm.Email,
                ContactInfo = vm.ContactInfo
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> UpdateCustomerInfo(int id, CustomerViewModel vm)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return null;

                customer.Name = vm.Name;
                customer.Email = vm.Email;
                customer.ContactInfo = vm.ContactInfo;

                await _context.SaveChangesAsync();
                return customer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Customer?> GetAccountDetails(int customerId)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Loans)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomers()
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .ToListAsync();
        }

        public async Task<bool> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
