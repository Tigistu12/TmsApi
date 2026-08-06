using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "text",
                nullable: false,
                defaultValue: "");

            // UPDATE EXISTING NULL RECORDS TO 'Pending'
        migrationBuilder.Sql(@"
            UPDATE ""Enrollments"" 
            SET ""Status"" = 'Pending' 
            WHERE ""Status"" IS NULL OR ""Status"" = '';
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");
        }
    }
}
