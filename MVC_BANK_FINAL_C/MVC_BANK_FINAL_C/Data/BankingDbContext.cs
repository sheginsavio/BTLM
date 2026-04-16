using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Models.Entities;

namespace MVC_BANK_FINAL_C.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Repayment> Repayments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer -> Account (one-to-many)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Account -> Transaction (one-to-many)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction -> AuditLog (one-to-one)
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.Transaction)
                .WithOne(t => t.AuditLog)
                .HasForeignKey<AuditLog>(al => al.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer -> Loan (one-to-many)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.Loans)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Loan -> Repayment (one-to-many)
            modelBuilder.Entity<Repayment>()
                .HasOne(r => r.Loan)
                .WithMany(l => l.Repayments)
                .HasForeignKey(r => r.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Customer (optional one-to-one, FK on User side)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Customer)
                .WithMany()
                .HasForeignKey(u => u.CustomerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Decimal precision ───────────────────────────────
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Loan>()
                .Property(l => l.LoanAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Loan>()
                .Property(l => l.InterestRate)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Loan>()
                .Property(l => l.MonthlyEMI)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Repayment>()
                .Property(r => r.AmountPaid)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Repayment>()
                .Property(r => r.BalanceRemaining)
                .HasColumnType("decimal(18,2)");

            // ── Seed default staff users ────────────────────────
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin",        Password = "admin123",  Role = "Admin",        CustomerId = null },
                new User { UserId = 2, Username = "teller1",      Password = "teller123", Role = "Teller",       CustomerId = null },
                new User { UserId = 3, Username = "loanofficer1", Password = "loan123",   Role = "LoanOfficer",  CustomerId = null },
                new User { UserId = 4, Username = "auditor1",     Password = "audit123",  Role = "Auditor",      CustomerId = null }
            );
        }
    }
}
