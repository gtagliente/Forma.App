using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forma.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class ExerciseResourceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseResource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Link = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercise_ExerciseResources",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Exercise_Name",
                table: "Exercise",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ExerciseResource_ExerciseId_ExerciseResourceId",
                table: "ExerciseResource",
                columns: new[] { "ExerciseId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ExerciseResource_Link",
                table: "ExerciseResource",
                column: "Link",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseResource");

            migrationBuilder.DropIndex(
                name: "UQ_Exercise_Name",
                table: "Exercise");
        }
    }
}
