using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class RebaseThirdTableFileToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDbTokenDb");

            migrationBuilder.CreateTable(
                name: "filetoken",
                columns: table => new
                {
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    token_id = table.Column<int>(type: "integer", nullable: false),
                    token_name = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filetoken", x => new { x.file_id, x.token_id, x.token_name });
                    table.ForeignKey(
                        name: "FK_filetoken_file_file_id",
                        column: x => x.file_id,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_filetoken_token_token_id_token_name",
                        columns: x => new { x.token_id, x.token_name },
                        principalTable: "token",
                        principalColumns: new[] { "id", "token_name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_filetoken_token_id_token_name",
                table: "filetoken",
                columns: new[] { "token_id", "token_name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filetoken");

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
                name: "IX_FileDbTokenDb_TokensId_TokensTokenName",
                table: "FileDbTokenDb",
                columns: new[] { "TokensId", "TokensTokenName" });
        }
    }
}
