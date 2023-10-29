using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "account",
                columns: new[] { "id", "login", "password", "role" },
                values: new object[,]
                {
                    { 1, "admin", "admin", 0 },
                    { 2, "user1", "user1", 1 },
                    { 3, "user2", "user2", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "account",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "account",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "account",
                keyColumn: "id",
                keyValue: 3);
        }
    }
}
