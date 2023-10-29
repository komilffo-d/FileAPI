using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accountid = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_type = table.Column<string>(type: "text", nullable: false),
                    shared = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file", x => x.id);
                    table.ForeignKey(
                        name: "FK_file_account_accountid",
                        column: x => x.accountid,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "token",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token_name = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token", x => new { x.id, x.token_name });
                    table.ForeignKey(
                        name: "FK_token_account_accountid",
                        column: x => x.accountid,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileDbTokenDb",
                columns: table => new
                {
                    FilesId = table.Column<int>(type: "integer", nullable: false),
                    TokensId = table.Column<int>(type: "integer", nullable: false),
                    TokensTokenName = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDbTokenDb", x => new { x.FilesId, x.TokensId, x.TokensTokenName });
                    table.ForeignKey(
                        name: "FK_FileDbTokenDb_file_FilesId",
                        column: x => x.FilesId,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileDbTokenDb_token_TokensId_TokensTokenName",
                        columns: x => new { x.TokensId, x.TokensTokenName },
                        principalTable: "token",
                        principalColumns: new[] { "id", "token_name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_file_accountid",
                table: "file",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_FileDbTokenDb_TokensId_TokensTokenName",
                table: "FileDbTokenDb",
                columns: new[] { "TokensId", "TokensTokenName" });

            migrationBuilder.CreateIndex(
                name: "IX_token_accountid",
                table: "token",
                column: "accountid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDbTokenDb");

            migrationBuilder.DropTable(
                name: "file");

            migrationBuilder.DropTable(
                name: "token");

            migrationBuilder.DropTable(
                name: "account");
        }
    }
}
