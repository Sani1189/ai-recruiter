using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Replace_JobPostCountryExposure_With_CountryExposureSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_CountryExposureSets_CountryExposureSetId",
                table: "JobPosts");

            // Drop foreign keys on JobAdCountryExposure before dropping the table
            migrationBuilder.DropForeignKey(
                name: "FK_JobAdCountryExposure_CountryExposureSets_CountryExposureSetId",
                table: "JobAdCountryExposure");

            migrationBuilder.DropForeignKey(
                name: "FK_JobAdCountryExposure_Country_CountryCode",
                table: "JobAdCountryExposure");

            migrationBuilder.DropForeignKey(
                name: "FK_JobAdCountryExposure_JobPosts_JobPostName_JobPostVersion",
                table: "JobAdCountryExposure");

            // Drop indexes on JobAdCountryExposure before dropping the table
            migrationBuilder.DropIndex(
                name: "IX_JobAdCountryExposure_CountryExposureSetId",
                table: "JobAdCountryExposure");

            migrationBuilder.DropIndex(
                name: "IX_JobAdCountryExposure_CountryCode",
                table: "JobAdCountryExposure");

            migrationBuilder.DropIndex(
                name: "IX_JobAdCountryExposure_JobPostName_JobPostVersion",
                table: "JobAdCountryExposure");

            migrationBuilder.DropTable(
                name: "JobAdCountryExposure");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_CountryExposureSets_CountryExposureSetId",
                table: "JobPosts",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_CountryExposureSets_CountryExposureSetId",
                table: "JobPosts");

            migrationBuilder.CreateTable(
                name: "JobAdCountryExposure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CountryCode = table.Column<string>(type: "nchar(2)", nullable: false),
                    CountryExposureSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobPostName = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    JobPostVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataOriginRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataResidency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSanitized = table.Column<bool>(type: "bit", nullable: true),
                    LastSyncEventId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SanitizationOverrideConsentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SanitizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAdCountryExposure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobAdCountryExposure_CountryExposureSets_CountryExposureSetId",
                        column: x => x.CountryExposureSetId,
                        principalTable: "CountryExposureSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JobAdCountryExposure_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "CountryCode");
                    table.ForeignKey(
                        name: "FK_JobAdCountryExposure_JobPosts_JobPostName_JobPostVersion",
                        columns: x => new { x.JobPostName, x.JobPostVersion },
                        principalTable: "JobPosts",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAdCountryExposure_CountryCode",
                table: "JobAdCountryExposure",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_JobAdCountryExposure_CountryExposureSetId",
                table: "JobAdCountryExposure",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobAdCountryExposure_JobPostName_JobPostVersion",
                table: "JobAdCountryExposure",
                columns: new[] { "JobPostName", "JobPostVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_CountryExposureSets_CountryExposureSetId",
                table: "JobPosts",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
