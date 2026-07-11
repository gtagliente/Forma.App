using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forma.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class ExerciseHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Exercise",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_ParentId",
                table: "Exercise",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercise_Parent",
                table: "Exercise",
                column: "ParentId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_Parent",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_ParentId",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Exercise");
        }
    }
}
