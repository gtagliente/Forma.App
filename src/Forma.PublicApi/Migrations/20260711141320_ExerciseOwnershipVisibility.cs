using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forma.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class ExerciseOwnershipVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Exercise_Name",
                table: "Exercise");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Exercise",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Exercise_Name_PerOwner",
                table: "Exercise",
                columns: new[] { "Name", "OwnerId" },
                unique: true,
                filter: "[OwnerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Exercise_Name_Shared",
                table: "Exercise",
                column: "Name",
                unique: true,
                filter: "[OwnerId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Exercise_Name_PerOwner",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "UQ_Exercise_Name_Shared",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Exercise");

            migrationBuilder.CreateIndex(
                name: "UQ_Exercise_Name",
                table: "Exercise",
                column: "Name",
                unique: true);
        }
    }
}
