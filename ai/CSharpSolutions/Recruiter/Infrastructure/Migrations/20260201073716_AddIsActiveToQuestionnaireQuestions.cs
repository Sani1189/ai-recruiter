using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToQuestionnaireQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireQuestions_QuestionnaireSectionId_Order",
                table: "QuestionnaireQuestions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "QuestionnaireQuestions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestions_QuestionnaireSectionId_Order",
                table: "QuestionnaireQuestions",
                columns: new[] { "QuestionnaireSectionId", "Order" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireQuestions_QuestionnaireSectionId_Order",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "QuestionnaireQuestions");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestions_QuestionnaireSectionId_Order",
                table: "QuestionnaireQuestions",
                columns: new[] { "QuestionnaireSectionId", "Order" },
                unique: true);
        }
    }
}
