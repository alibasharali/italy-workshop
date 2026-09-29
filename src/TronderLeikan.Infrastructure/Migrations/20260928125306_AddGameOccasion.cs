using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TronderLeikan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameOccasion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Occasion",
                table: "Games",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Occasion",
                table: "Games");
        }
    }
}
