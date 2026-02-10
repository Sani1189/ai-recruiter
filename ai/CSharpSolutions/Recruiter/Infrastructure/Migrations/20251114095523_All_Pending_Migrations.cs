using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class All_Pending_Migrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: All changes (Comments, Duration, InterviewQuestions) were already handled in previous migrations
            // These indexes are in DbContext configuration but weren't in previous migrations
            // If they already exist in database, these statements will be skipped (defensive)
            // If they don't exist, they will be created now to sync DbContext with database
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'VolunteerExtracurricular')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VolunteerExtracurricular_StartDate' 
                                   AND object_id = OBJECT_ID('VolunteerExtracurricular'))
                    BEGIN
                        CREATE INDEX IX_VolunteerExtracurricular_StartDate ON VolunteerExtracurricular(StartDate);
                    END
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProjectsResearch')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectsResearch_Role' 
                                   AND object_id = OBJECT_ID('ProjectsResearch'))
                    BEGIN
                        CREATE INDEX IX_ProjectsResearch_Role ON ProjectsResearch(Role);
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VolunteerExtracurricular_StartDate' 
                           AND object_id = OBJECT_ID('VolunteerExtracurricular'))
                BEGIN
                    DROP INDEX IX_VolunteerExtracurricular_StartDate ON VolunteerExtracurricular;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectsResearch_Role' 
                           AND object_id = OBJECT_ID('ProjectsResearch'))
                BEGIN
                    DROP INDEX IX_ProjectsResearch_Role ON ProjectsResearch;
                END
            ");
        }
    }
}
