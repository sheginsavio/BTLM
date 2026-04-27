using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_BANK_FINAL_C.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordsToBCrypt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$l2rXFuxAFET9BUGQVizYD.cY0q5TVYsngGQchQuLHOVZGAVu2MCoq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$cb4FQpil0DiswslbxWEL8OIMFxbMfhiMuj0O9hgrM8js6uYIi1k8G");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "$2a$12$seF.cDgkGBfUtVfTcfsZJONJFBL2AibfysFJjjqvwxPf0c9rSMY4q");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "$2a$12$fEX/9J2.dePY7oKY0o8j0uvOIK0ELBUWJEjmo4WRi3Wejma.//2Au");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "d277f25b136e402488431a226b652b2bff919188028af250d9dab860e2c436fd");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "6e780d72b4e5bf4f9f098c7a1d9821dd9233d7aab1e9b32d5a4da67e4168cf76");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "0b26f7caa1c2e5e3f11adfd22f47403ed214a1d4451117a18ea726b451a3aa61");
        }
    }
}
