using Bank_Transaction_and_Loan_management_System.Models;

namespace Bank_Transaction_and_Loan_management_System.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<List<Customer>> GetAllCustomersAsync();
    }
}
