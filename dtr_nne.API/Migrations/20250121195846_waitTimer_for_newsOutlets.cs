using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dtr_nne.Migrations
{
    /// <inheritdoc />
    public partial class waitTimer_for_newsOutlets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WaitTimer",
                table: "NewsOutlets",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaitTimer",
                table: "NewsOutlets");
        }
    }
}
