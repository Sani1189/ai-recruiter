using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_JobPost_OriginCountryId_And_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginCountryId",
                table: "JobPosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "JobPosts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_OriginCountryId",
                table: "JobPosts",
                column: "OriginCountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_Country_OriginCountryId",
                table: "JobPosts",
                column: "OriginCountryId",
                principalTable: "Country",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_Country_OriginCountryId",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_OriginCountryId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "OriginCountryId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "JobPosts");
        }
    }
}
