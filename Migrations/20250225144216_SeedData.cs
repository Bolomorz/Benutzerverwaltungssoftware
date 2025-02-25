using Microsoft.EntityFrameworkCore.Migrations;
using Benutzerverwaltungssoftware.Security;

#nullable disable

namespace Benutzerverwaltungssoftware.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var admin = Hashing.GenerateUserData("d.schneider", "!Fn3ika4a?").ReturnValue;
            var user = Hashing.GenerateUserData("a.schneider", "!s@b9SuX").ReturnValue;

            migrationBuilder.InsertData(
                table: "UserAccounts",
                columns: new[] { "UserAccountID", "Name", "PasswordHash", "HashParameter", "Role"},
                values: new object[] { 1, admin.Name, admin.PasswordHash, admin.HashParameter, 1}
            );
            migrationBuilder.InsertData(
                table: "UserAccounts",
                columns: new[] { "UserAccountID", "Name", "PasswordHash", "HashParameter", "Role"},
                values: new object[] { 2, user.Name, user.PasswordHash, user.HashParameter, 0}
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
