using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> CreateAccount(CustomerViewModel vm);
        Task<Customer?> UpdateCustomerInfo(int id, CustomerViewModel vm);
        Task<Customer?> GetAccountDetails(int customerId);
        Task<IEnumerable<Customer>> GetAllCustomers();
        Task<bool> DeleteCustomer(int id);
    }
}
