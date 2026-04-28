using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Helpers;
using MVC_BANK_FINAL_C.Models.Entities;

namespace MVC_BANK_FINAL_C.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(BankingDbContext context)
        {
            // Only seed if no users exist yet
            if (await context.Users.AnyAsync()) return;

            var users = new List<User>
            {
                new User
                {
                    Username     = "admin",
                    Password     = PasswordHelper.HashPassword("admin123"),
                    Role         = "Admin",
                    CustomerId   = null,
                    IsFirstLogin = false
                },
                new User
                {
                    Username     = "teller1",
                    Password     = PasswordHelper.HashPassword("teller123"),
                    Role         = "Teller",
                    CustomerId   = null,
                    IsFirstLogin = false
                },
                new User
                {
                    Username     = "loanofficer1",
                    Password     = PasswordHelper.HashPassword("loan123"),
                    Role         = "LoanOfficer",
                    CustomerId   = null,
                    IsFirstLogin = false
                },
                new User
                {
                    Username     = "auditor1",
                    Password     = PasswordHelper.HashPassword("audit123"),
                    Role         = "Auditor",
                    CustomerId   = null,
                    IsFirstLogin = false
                }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }
    }
}
