using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeJobStepVersion_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostStepAssignments_JobPostSteps_StepName_StepVersion",
                table: "JobPostStepAssignments");

            migrationBuilder.DropIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion_StepName_StepVersion",
                table: "JobPostStepAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "StepVersion",
                table: "JobPostStepAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "JobPostStepName",
                table: "JobPostStepAssignments",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobPostStepVersion",
                table: "JobPostStepAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion_StepName_StepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostName", "JobPostVersion", "StepName", "StepVersion" },
                unique: true,
                filter: "[StepVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_JobPostStepName_JobPostStepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostStepName", "JobPostStepVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostStepAssignments_JobPostSteps_JobPostStepName_JobPostStepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostStepName", "JobPostStepVersion" },
                principalTable: "JobPostSteps",
                principalColumns: new[] { "Name", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostStepAssignments_JobPostSteps_JobPostStepName_JobPostStepVersion",
                table: "JobPostStepAssignments");

            migrationBuilder.DropIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion_StepName_StepVersion",
                table: "JobPostStepAssignments");

            migrationBuilder.DropIndex(
                name: "IX_JobPostStepAssignments_JobPostStepName_JobPostStepVersion",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "JobPostStepName",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "JobPostStepVersion",
                table: "JobPostStepAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "StepVersion",
                table: "JobPostStepAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion_StepName_StepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostName", "JobPostVersion", "StepName", "StepVersion" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostStepAssignments_JobPostSteps_StepName_StepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "StepName", "StepVersion" },
                principalTable: "JobPostSteps",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
