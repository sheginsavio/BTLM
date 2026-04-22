using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;

namespace Tests.Helpers
{
    public static class DbContextMockHelper
    {
        public static BankingDbContext CreateInMemoryContext(string dbName = "TestDb")
        {
            var options = new DbContextOptionsBuilder<BankingDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid().ToString())
                .Options;
            return new BankingDbContext(options);
        }
    }
}
