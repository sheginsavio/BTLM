using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_BANK_FINAL_C.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditAccountToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditAccountId",
                table: "Loans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_CreditAccountId",
                table: "Loans",
                column: "CreditAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Accounts_CreditAccountId",
                table: "Loans",
                column: "CreditAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Accounts_CreditAccountId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_CreditAccountId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "CreditAccountId",
                table: "Loans");
        }
    }
}
