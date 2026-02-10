using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKanbanBoardColumnsAndJobPostFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create KanbanBoardColumns table
            migrationBuilder.CreateTable(
                name: "KanbanBoardColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecruiterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanBoardColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanbanBoardColumns_UserProfiles_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create unique index on (RecruiterId, Sequence)
            migrationBuilder.CreateIndex(
                name: "IX_KanbanBoardColumns_RecruiterId_Sequence",
                table: "KanbanBoardColumns",
                columns: new[] { "RecruiterId", "Sequence" },
                unique: true);

            // Add new columns to JobPosts table
            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "JobPosts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IntroText",
                table: "JobPosts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatWeOffer",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyInfo",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentBoardColumnId",
                table: "JobPosts",
                type: "uniqueidentifier",
                nullable: true);

            // Create foreign key index for CurrentBoardColumnId
            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_CurrentBoardColumnId",
                table: "JobPosts",
                column: "CurrentBoardColumnId");

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_KanbanBoardColumns_CurrentBoardColumnId",
                table: "JobPosts",
                column: "CurrentBoardColumnId",
                principalTable: "KanbanBoardColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_KanbanBoardColumns_CurrentBoardColumnId",
                table: "JobPosts");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_JobPosts_CurrentBoardColumnId",
                table: "JobPosts");

            // Drop columns from JobPosts
            migrationBuilder.DropColumn(
                name: "Industry",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "IntroText",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "WhatWeOffer",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "CompanyInfo",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "CurrentBoardColumnId",
                table: "JobPosts");

            // Drop KanbanBoardColumns table
            migrationBuilder.DropTable(
                name: "KanbanBoardColumns");
        }
    }
}
