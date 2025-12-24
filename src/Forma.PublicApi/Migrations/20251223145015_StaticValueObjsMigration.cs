using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forma.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class StaticValueObjsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaticValueObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticValueObjects", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaticValueObjects");
        }
    }
}
