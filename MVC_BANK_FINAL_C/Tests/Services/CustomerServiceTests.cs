using NUnit.Framework;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Implementations;
using Tests.Helpers;

namespace Tests.Services
{
    [TestFixture]
    public class CustomerServiceTests
    {
        private CustomerService _service;

        [SetUp]
        public void SetUp()
        {
            var context = DbContextMockHelper.CreateInMemoryContext("CustomerDb");
            _service = new CustomerService(context);
        }

        [Test]
        public async Task CreateAccount_ValidViewModel_ReturnsCustomerWithCorrectName()
        {
            var vm = new CustomerViewModel
            {
                Name        = "John Doe",
                Email       = "john@example.com",
                ContactInfo = "9876543210"
            };

            var result = await _service.CreateAccount(vm);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("John Doe"));
            Assert.That(result.Email, Is.EqualTo("john@example.com"));
        }

        [Test]
        public async Task CreateAccount_ValidViewModel_AssignsCustomerId()
        {
            var vm = new CustomerViewModel
            {
                Name        = "Jane Doe",
                Email       = "jane@example.com",
                ContactInfo = "1234567890"
            };

            var result = await _service.CreateAccount(vm);

            Assert.That(result.CustomerId, Is.GreaterThan(0));
        }

        [Test]
        public async Task UpdateCustomerInfo_ExistingCustomer_UpdatesNameAndEmail()
        {
            // Arrange: create a customer first
            var createVm = new CustomerViewModel
            {
                Name        = "Old Name",
                Email       = "old@example.com",
                ContactInfo = "0000000000"
            };
            var created = await _service.CreateAccount(createVm);

            // Act: update
            var updateVm = new CustomerViewModel
            {
                Name        = "New Name",
                Email       = "new@example.com",
                ContactInfo = "1111111111"
            };
            var updated = await _service.UpdateCustomerInfo(created.CustomerId, updateVm);

            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Name, Is.EqualTo("New Name"));
            Assert.That(updated.Email, Is.EqualTo("new@example.com"));
        }

        [Test]
        public async Task UpdateCustomerInfo_NonExistingCustomer_ReturnsNull()
        {
            var vm = new CustomerViewModel
            {
                Name        = "Ghost",
                Email       = "ghost@example.com",
                ContactInfo = "0000000000"
            };

            var result = await _service.UpdateCustomerInfo(9999, vm);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllCustomers_ReturnsAllCreatedCustomers()
        {
            await _service.CreateAccount(new CustomerViewModel { Name = "A", Email = "a@a.com" });
            await _service.CreateAccount(new CustomerViewModel { Name = "B", Email = "b@b.com" });

            var all = await _service.GetAllCustomers();

            Assert.That(all.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteCustomer_ExistingCustomer_ReturnsTrue()
        {
            var created = await _service.CreateAccount(new CustomerViewModel
            {
                Name  = "To Delete",
                Email = "del@del.com"
            });

            var result = await _service.DeleteCustomer(created.CustomerId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteCustomer_NonExistingCustomer_ReturnsFalse()
        {
            var result = await _service.DeleteCustomer(9999);

            Assert.That(result, Is.False);
        }
    }
}
