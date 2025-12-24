using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forma.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class muscleGroupsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleGroup",
                table: "Exercise");

            migrationBuilder.AddColumn<string>(
                name: "MuscleGroups",
                table: "Exercise",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleGroups",
                table: "Exercise");

            migrationBuilder.AddColumn<string>(
                name: "MuscleGroup",
                table: "Exercise",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
