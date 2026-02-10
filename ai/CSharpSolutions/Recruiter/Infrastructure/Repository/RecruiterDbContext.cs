using Microsoft.EntityFrameworkCore;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Extensions;
using Recruiter.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using File = Recruiter.Domain.Models.File;
using Recruiter.Domain.Enums;

namespace Recruiter.Infrastructure.Repository;

public class RecruiterDbContext : DbContext
{
    public RecruiterDbContext(DbContextOptions<RecruiterDbContext> options) : base(options)
    {
    }

    public DbSet<JobPost> JobPosts { get; set; } = null!;
    public DbSet<JobPostStep> JobPostSteps { get; set; } = null!;
    public DbSet<JobPostStepAssignment> JobPostStepAssignments { get; set; } = null!;
    public DbSet<CountryExposureSet> CountryExposureSets { get; set; } = null!;
    public DbSet<CountryExposureSetCountry> CountryExposureSetCountries { get; set; } = null!;

    // JobApplication entities
    public DbSet<JobApplication> JobApplications { get; set; } = null!;
    public DbSet<JobApplicationStep> JobApplicationSteps { get; set; } = null!;
    public DbSet<JobApplicationStepFiles> JobApplicationStepFiles { get; set; } = null!;
    public DbSet<Interview> Interviews { get; set; } = null!;
    public DbSet<Score> Scores { get; set; } = null!;
    public DbSet<Feedback> Feedbacks { get; set; } = null!;

    // Additional entities
    public DbSet<Country> Country { get; set; } = null!;
    public DbSet<Candidate> Candidates { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<File> Files { get; set; } = null!;
    public DbSet<InterviewConfiguration> InterviewConfigurations { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;

    // Questionnaire templates
    public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; } = null!;
    public DbSet<QuestionnaireSection> QuestionnaireSections { get; set; } = null!;
    public DbSet<QuestionnaireQuestion> QuestionnaireQuestions { get; set; } = null!;
    public DbSet<QuestionnaireQuestionOption> QuestionnaireQuestionOptions { get; set; } = null!;

    // Candidate questionnaire submissions
    public DbSet<QuestionnaireCandidateSubmission> QuestionnaireCandidateSubmissions { get; set; } = null!;
    public DbSet<QuestionnaireCandidateSubmissionAnswer> QuestionnaireCandidateSubmissionAnswers { get; set; } = null!;
    public DbSet<QuestionnaireCandidateSubmissionAnswerOption> QuestionnaireCandidateSubmissionAnswerOptions { get; set; } = null!;

    public DbSet<KeyStrength> KeyStrengths { get; set; } = null!;
    public DbSet<Education> Educations { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<Experience> Experiences { get; set; } = null!;
    public DbSet<CvEvaluation> CvEvaluations { get; set; } = null!;
    public DbSet<AwardAchievement> AwardAchievements { get; set; } = null!;
    public DbSet<CertificationLicense> CertificationLicenses { get; set; } = null!;
    public DbSet<Summary> Summaries { get; set; } = null!;
    public DbSet<Scoring> Scorings { get; set; } = null!;
    public DbSet<VolunteerExtracurricular> VolunteerExtracurriculars { get; set; } = null!;
    public DbSet<ProjectResearch> ProjectResearches { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    // Sync configuration
    public DbSet<EntitySyncConfiguration> EntitySyncConfigurations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CountryExposureSet>(e =>
        {
            e.ToTable("CountryExposureSets");
            e.HasKey(x => x.Id);
            e.ConfigureBasicBaseDbModelProperties();

            // Optional but useful: also enforce canonical uniqueness
            e.Property(x => x.Canonical).IsRequired();
            e.HasIndex(x => x.Canonical).IsUnique();

            e.HasMany(x => x.Countries)
             .WithOne(x => x.CountryExposureSet)
             .HasForeignKey(x => x.CountryExposureSetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CountryExposureSetCountry>(e =>
        {
            e.ToTable("CountryExposureSetCountries");
            e.HasKey(x => x.Id);
            e.ConfigureBasicBaseDbModelProperties();

            e.Property(x => x.CountryCode)
             .IsRequired()
             .HasMaxLength(2)
             .IsFixedLength();

            e.HasIndex(x => new { x.CountryExposureSetId, x.CountryCode })
             .IsUnique();

            e.HasOne(x => x.Country)
             .WithMany()
             .HasForeignKey(x => x.CountryCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CountryExposureSet)
             .WithMany(x => x.Countries)
             .HasForeignKey(x => x.CountryExposureSetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure JobPost entity
        modelBuilder.Entity<JobPost>(entity =>
        {
            // Configure VersionedBaseDbModel properties
            entity.ConfigureVersionedBaseDbModelProperties();

            // Configure JobPost-specific properties
            entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(200);
            entity.Property(e => e.JobDescription).IsRequired();
            entity.Property(e => e.JobType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExperienceLevel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MaxAmountOfCandidatesRestriction).IsRequired();
            entity.Property(e => e.MinimumRequirements).IsRequired()
                 .HasConversion(
                     v => string.Join("|", v),
                     v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList()
                 )
                 .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                     (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                     c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                     c => c.ToList()
                 ));
            entity.Property(e => e.PoliceReportRequired);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();
            entity.Property(e => e.OriginCountryCode)
                .HasMaxLength(2)
                .IsFixedLength();

            entity
                .HasOne(e => e.OriginCountry)
                .WithMany()
                .HasForeignKey(e => e.OriginCountryCode)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.ExperienceLevel);
            entity.HasIndex(e => e.JobType);
            entity.HasIndex(e => e.OriginCountryCode);
            entity
                .HasOne(e => e.CountryExposureSet)
                .WithMany()
                .HasForeignKey(e => e.CountryExposureSetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.Name, e.Version });
            entity.HasIndex(e => e.CountryExposureSetId);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ConfigureBasicBaseDbModelProperties();
            entity.HasKey(e => e.CountryCode);

            entity.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength();

            entity.HasData(CountrySeedData.GetAll());
        });

        // Configure JobPostStep entity
        modelBuilder.Entity<JobPostStep>(entity =>
        {
            // Configure VersionedBaseDbModel properties
            entity.ConfigureVersionedBaseDbModelProperties();

            // Configure JobPostStep-specific properties
            entity.Property(e => e.IsInterview).IsRequired();
            entity.Property(e => e.StepType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Participant)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Candidate");
            entity.Property(e => e.ShowStepForCandidate).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DisplayTitle).HasMaxLength(255);
            entity.Property(e => e.DisplayContent);
            entity.Property(e => e.ShowSpinner).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.InterviewConfigurationName).HasMaxLength(255);
            entity.Property(e => e.InterviewConfigurationVersion);
            entity.Property(e => e.PromptName).HasMaxLength(255);
            entity.Property(e => e.PromptVersion);

            entity.Property(e => e.QuestionnaireTemplateName).HasMaxLength(255);
            entity.Property(e => e.QuestionnaireTemplateVersion);

            // Configure relationships according to Models.txt
            // Ref: JobPostStep.(interviewConfigurationName, interviewConfigurationVersion) > InterviewConfiguration.(name, version)
            entity.HasOne(e => e.InterviewConfiguration)
                .WithMany()
                .HasForeignKey(e => new { e.InterviewConfigurationName, e.InterviewConfigurationVersion })
                .HasPrincipalKey(ic => new { ic.Name, ic.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: JobPostStep.(promptName, promptVersion) > Prompt.(name, version)
            entity.HasOne(e => e.Prompt)
                .WithMany()
                .HasForeignKey(e => new { e.PromptName, e.PromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: JobPostStep.(QuestionnaireTemplateName, QuestionnaireTemplateVersion) > QuestionnaireTemplate.(name, version)
            entity.HasOne(e => e.QuestionnaireTemplate)
                .WithMany()
                .HasForeignKey(e => new { e.QuestionnaireTemplateName, e.QuestionnaireTemplateVersion })
                .HasPrincipalKey(t => new { t.Name, t.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Create indexes
            entity.HasIndex(e => e.StepType);
            entity.HasIndex(e => e.IsInterview);
            entity.HasIndex(e => e.Participant);
            entity.HasIndex(e => new { e.InterviewConfigurationName, e.InterviewConfigurationVersion });
            entity.HasIndex(e => new { e.PromptName, e.PromptVersion });
            entity.HasIndex(e => new { e.QuestionnaireTemplateName, e.QuestionnaireTemplateVersion });
        });

        // Configure JobPostStepAssignment entity (junction table)
        modelBuilder.Entity<JobPostStepAssignment>(entity =>
        {
            // Configure base model properties (Id, Created, Updated, RowVersion, etc.)
            entity.ConfigureBaseDbModelProperties();

            // Configure composite unique key (not primary key since we have Id as primary key)
            entity.HasIndex(e => new { e.JobPostName, e.JobPostVersion, e.StepName, e.StepVersion })
                .IsUnique();

            // Configure properties
            entity.Property(e => e.JobPostName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.JobPostVersion).IsRequired();
            entity.Property(e => e.StepName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StepVersion); // Nullable - null means "use latest version dynamically"
            entity.Property(e => e.StepNumber).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("pending");

            // Configure relationships with navigation properties
            entity.HasOne(e => e.JobPost)
                .WithMany(jp => jp.StepAssignments)
                .HasForeignKey(e => new { e.JobPostName, e.JobPostVersion })
                .OnDelete(DeleteBehavior.Cascade);

            // Don't configure FK relationship to JobPostStep because StepVersion can be null
            // When StepVersion is null, it means "use latest version dynamically"
            // We handle step loading manually in the service layer
            // entity.HasOne(e => e.JobPostStep)
            //     .WithMany(jps => jps.StepAssignments)
            //     .HasForeignKey(e => new { e.StepName, e.StepVersion })
            //     .OnDelete(DeleteBehavior.Restrict);

            // Create indexes for queries
            entity.HasIndex(e => new { e.JobPostName, e.JobPostVersion });
            entity.HasIndex(e => new { e.StepName, e.StepVersion });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.JobPostName, e.JobPostVersion, e.StepNumber });
        });

        // Configure JobApplication entity
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.JobPostName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.JobPostVersion).IsRequired();
            entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(255);

            // Configure relationships according to Models.txt
            // Ref: JobApplication.(jobPostName, jobPostVersion) > JobPost.(name, version)
            entity.HasOne(e => e.JobPost)
                .WithMany()
                .HasForeignKey(e => new { e.JobPostName, e.JobPostVersion })
                .HasPrincipalKey(jp => new { jp.Name, jp.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: JobApplication.candidateId > Candidate.id
            entity.HasOne(e => e.Candidate)
                .WithMany()
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create indexes
            entity.HasIndex(e => new { e.JobPostName, e.JobPostVersion });
            entity.HasIndex(e => e.CandidateId);
        });

        // Configure JobApplicationStep entity
        modelBuilder.Entity<JobApplicationStep>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.JobApplicationId).IsRequired();
            entity.Property(e => e.JobPostStepName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.JobPostStepVersion).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(JobApplicationStepStatusEnum.Pending);
            entity.Property(e => e.StepNumber).IsRequired();
            entity.Property(e => e.Data);

            // Configure relationships according to Models.txt
            // Ref: JobApplicationStep.jobApplicationId > JobApplication.id
            entity.HasOne(e => e.JobApplication)
                .WithMany(ja => ja.Steps)
                .HasForeignKey(e => e.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ref: JobApplicationStep.(jobPostStepName, jobPostStepVersion) > JobPostStep.(name, version)
            entity.HasOne(e => e.JobPostStep)
                .WithMany()
                .HasForeignKey(e => new { e.JobPostStepName, e.JobPostStepVersion })
                .HasPrincipalKey(jps => new { jps.Name, jps.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // 1:1 relationship with Interview (configured in Interview entity)
            // This is a one-way navigation - the foreign key is in Interview table

            // Create indexes
            entity.HasIndex(e => e.JobApplicationId);
            entity.HasIndex(e => new { e.JobPostStepName, e.JobPostStepVersion });
            entity.HasIndex(e => e.Status);
        });

        // Configure JobApplicationStepFiles entity
        modelBuilder.Entity<JobApplicationStepFiles>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.JobApplicationStepId).IsRequired();

            // Configure relationships
            // Ref: JobApplicationStepFiles.fileId - File.id (1:1 from JobApplicationStepFiles perspective)
            entity.HasOne(e => e.File)
                .WithMany()  // No inverse navigation in File
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ref: JobApplicationStepFiles.jobApplicationStepId > JobApplicationStep.id (Many:1)
            entity.HasOne(e => e.JobApplicationStep)
                .WithMany(jas => jas.JobApplicationStepFiles)
                .HasForeignKey(e => e.JobApplicationStepId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.FileId);
            entity.HasIndex(e => e.JobApplicationStepId);
            entity.HasIndex(e => new { e.FileId, e.JobApplicationStepId }).IsUnique();
        });

        // Configure Interview entity
        modelBuilder.Entity<Interview>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.JobApplicationStepId).IsRequired();
            entity.Property(e => e.InterviewAudioUrl).HasMaxLength(500);
            entity.Property(e => e.InterviewConfigurationName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.InterviewConfigurationVersion).IsRequired();
            entity.Property(e => e.TranscriptUrl).HasMaxLength(500);
            entity.Property(e => e.Duration);

            // Prompt properties
            entity.Property(e => e.InstructionPromptName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PersonalityPromptName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionsPromptName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.InstructionPromptVersion).IsRequired();
            entity.Property(e => e.PersonalityPromptVersion).IsRequired();
            entity.Property(e => e.QuestionsPromptVersion).IsRequired();

            // Configure relationships according to Models.txt
            // Ref: Interview.jobApplicationStepId - JobApplicationStep.id
            entity.HasOne(e => e.JobApplicationStep)
                .WithOne(jas => jas.Interview)
                .HasForeignKey<Interview>(e => e.JobApplicationStepId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ref: Interview.(interviewConfigurationName, interviewConfigurationVersion) > InterviewConfiguration.(name, version)
            entity.HasOne(e => e.InterviewConfiguration)
                .WithMany()
                .HasForeignKey(e => new { e.InterviewConfigurationName, e.InterviewConfigurationVersion })
                .HasPrincipalKey(ic => new { ic.Name, ic.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: Interview.(instructionPromptName, instructionPromptVersion) - Prompt.(name, version)
            entity.HasOne(e => e.InstructionPrompt)
                .WithMany()
                .HasForeignKey(e => new { e.InstructionPromptName, e.InstructionPromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: Interview.(personalityPromptName, personalityPromptVersion) - Prompt.(name, version)
            entity.HasOne(e => e.PersonalityPrompt)
                .WithMany()
                .HasForeignKey(e => new { e.PersonalityPromptName, e.PersonalityPromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: Interview.(questionsPromptName, questionsPromptVersion) - Prompt.(name, version)
            entity.HasOne(e => e.QuestionsPrompt)
                .WithMany()
                .HasForeignKey(e => new { e.QuestionsPromptName, e.QuestionsPromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict);

            // Create indexes
            entity.HasIndex(e => e.JobApplicationStepId);
            entity.HasIndex(e => new { e.InterviewConfigurationName, e.InterviewConfigurationVersion });
            entity.HasIndex(e => e.InstructionPromptName);
            entity.HasIndex(e => e.PersonalityPromptName);
            entity.HasIndex(e => e.QuestionsPromptName);
        });

        // Configure Score entity
        modelBuilder.Entity<Score>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.Average).IsRequired();
            entity.Property(e => e.English).IsRequired();
            entity.Property(e => e.Technical).IsRequired();
            entity.Property(e => e.Communication).IsRequired();
            entity.Property(e => e.ProblemSolving).IsRequired();
            entity.Property(e => e.InterviewId).IsRequired();

            // Configure relationships according to Models.txt
            // Ref: Score.interviewId - Interview.id
            entity.HasOne(e => e.Interview)
                .WithOne(i => i.Score)
                .HasForeignKey<Score>(e => e.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.InterviewId);
        });

        // Configure Feedback entity
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.Detailed).IsRequired();
            entity.Property(e => e.Summary).IsRequired();
            entity.Property(e => e.InterviewId).IsRequired();

            // Configure List<string> conversions
            entity.Property(e => e.Strengths)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>((
                    c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.Weaknesses)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>((
                    c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Configure relationships according to Models.txt
            // Ref: Feedback.interviewId - Interview.id
            entity.HasOne(e => e.Interview)
                .WithOne(i => i.Feedback)
                .HasForeignKey<Feedback>(e => e.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.InterviewId);
        });

        // Configure Candidate entity
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.CvFileId); // Optional - candidates can apply without CV
            entity.Property(e => e.UserId).IsRequired();

            // Configure relationships according to updated models
            // Ref: Candidate.userId - UserProfile.id (1:1)
            entity.HasOne(e => e.UserProfile)
                .WithOne(u => u.Candidate)
                .HasForeignKey<Candidate>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: Candidate.cvFileId - File.id (1:1)
            entity.HasOne(e => e.CvFile)
                .WithMany()
                .HasForeignKey(e => e.CvFileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create indexes
            entity.HasIndex(e => e.UserId)
                .IsUnique(); // One UserProfile can only have one Candidate
            entity.HasIndex(e => e.CvFileId);
        });

        // Configure UserProfile entity
        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Nationality)
                .HasMaxLength(100);

            entity.Property(e => e.ProfilePictureUrl)
                .HasMaxLength(500);

            entity.Property(e => e.ResumeUrl)
                .HasMaxLength(500);

            entity.Property(e => e.JobTypePreferences)
                .HasConversion(
                    v => string.Join(',', v ?? new List<string>()),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasDefaultValueSql("''")          // ✅ Fix: use SQL default literal
                .IsRequired(false)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.RemotePreferences)
                .HasConversion(
                    v => string.Join(',', v ?? new List<string>()),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasDefaultValueSql("''")          // ✅ same here
                .IsRequired(false)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.Roles)
                .HasConversion(
                    v => string.Join(',', v ?? new List<string>()),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasDefaultValueSql("''")
                .IsRequired(false)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Configure OpenToRelocation - property is nullable to handle NULL from database
            // Column is nullable in database (from older migrations), but defaults to false
            // ValueGeneratedNever ensures EF Core always includes the value in INSERT statements
            entity.Property(e => e.OpenToRelocation)
                .IsRequired(false) // Column is nullable in database
                .HasDefaultValue(false) // Default value for new records
                .ValueGeneratedNever(); // Always include value in INSERT, don't rely on database default

            // Create indexes
            entity.HasIndex(e => e.Email)
                .IsUnique(); // Email must be unique - one email per user profile
            entity.HasIndex(e => e.Name);
        });

        // Configure File entity
        modelBuilder.Entity<Domain.Models.File>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.Container).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FolderPath).HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Extension).IsRequired().HasMaxLength(10);
            entity.Property(e => e.MbSize).IsRequired();
            entity.Property(e => e.StorageAccountName).IsRequired().HasMaxLength(255);

            // Create indexes
            entity.HasIndex(e => e.Container);
            entity.HasIndex(e => e.FolderPath);
            entity.HasIndex(e => e.Extension);
            entity.HasIndex(e => new { e.Container, e.FolderPath });
        });

        // Configure InterviewConfiguration entity
        modelBuilder.Entity<InterviewConfiguration>(entity =>
        {
            entity.ConfigureVersionedBaseDbModelProperties();

            entity.Property(e => e.Modality).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Tone).HasMaxLength(100);
            entity.Property(e => e.ProbingDepth).HasMaxLength(100);
            entity.Property(e => e.FocusArea).HasMaxLength(100);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.InstructionPromptName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.InstructionPromptVersion);
            entity.Property(e => e.PersonalityPromptName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PersonalityPromptVersion);
            entity.Property(e => e.QuestionsPromptName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionsPromptVersion);
            entity.Property(e => e.Active).IsRequired().HasDefaultValue(true);

            // Configure relationships according to Models.txt
            // Ref: InterviewConfiguration.(instructionPromptName, instructionPromptVersion) > Prompt.(name, version)
            entity.HasOne(e => e.InstructionPrompt)
                .WithMany()
                .HasForeignKey(e => new { e.InstructionPromptName, e.InstructionPromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Ref: InterviewConfiguration.(personalityPromptName, personalityPromptVersion) > Prompt.(name, version)
            entity.HasOne(e => e.PersonalityPrompt)
                .WithMany()
                .HasForeignKey(e => new { e.PersonalityPromptName, e.PersonalityPromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Ref: InterviewConfiguration.(questionsPromptName, questionsPromptVersion) > Prompt.(name, version)
            entity.HasOne(e => e.QuestionsPrompt)
                .WithMany()
                .HasForeignKey(e => new { e.QuestionsPromptName, e.QuestionsPromptVersion })
                .HasPrincipalKey(p => new { p.Name, p.Version })
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Create indexes
            entity.HasIndex(e => e.Modality);
            entity.HasIndex(e => e.Active);
            entity.HasIndex(e => e.InstructionPromptName);
            entity.HasIndex(e => e.PersonalityPromptName);
            entity.HasIndex(e => e.QuestionsPromptName);
        });

        // Configure Prompt entity
        modelBuilder.Entity<Prompt>(entity =>
        {
            entity.ConfigureVersionedBaseDbModelProperties();

            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Locale).HasMaxLength(10);

            // Configure List<string> conversion
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>((
                    c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Create indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Locale);
        });

        // Configure QuestionnaireTemplate entity (versioned aggregate root)
        modelBuilder.Entity<QuestionnaireTemplate>(entity =>
        {
            entity.ConfigureVersionedBaseDbModelProperties();

            entity.Property(e => e.TemplateType)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Draft");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Description);
            entity.Property(e => e.PublishedAt);
            entity.Property(e => e.TimeLimitSeconds);

            entity.HasMany(e => e.Sections)
                .WithOne(s => s.QuestionnaireTemplate)
                .HasForeignKey(s => new { s.QuestionnaireTemplateName, s.QuestionnaireTemplateVersion })
                .HasPrincipalKey(t => new { t.Name, t.Version })
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TemplateType);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<QuestionnaireSection>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.QuestionnaireTemplateName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionnaireTemplateVersion).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description);

            entity.HasIndex(e => new { e.QuestionnaireTemplateName, e.QuestionnaireTemplateVersion, e.Order }).IsUnique();

            entity.HasMany(e => e.Questions)
                .WithOne()
                .HasForeignKey(q => q.QuestionnaireSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionnaireQuestion>(entity =>
        {
            entity.ConfigureVersionedBaseDbModelProperties();

            entity.Property(e => e.QuestionnaireSectionId).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.QuestionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.IsRequired).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.TraitKey).HasMaxLength(100);
            entity.Property(e => e.Ws).HasPrecision(18, 4);
            entity.Property(e => e.MediaUrl).HasMaxLength(1000);

            // Allow historical versions of the same question to coexist in the same section with the same Order.
            // Enforce uniqueness only for the "active" question row.
            entity.HasIndex(e => new { e.QuestionnaireSectionId, e.Order })
                .IsUnique()
                .HasFilter("[IsActive] = 1");
            entity.HasIndex(e => e.MediaFileId).IsUnique();

            // Ref: QuestionnaireQuestion.mediaFileId - File.id (1:1 per-table uniqueness)
            entity.HasOne(e => e.MediaFile)
                .WithMany()
                .HasForeignKey(e => e.MediaFileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Options)
                .WithOne(o => o.QuestionnaireQuestion)
                .HasForeignKey(o => new { o.QuestionnaireQuestionName, o.QuestionnaireQuestionVersion })
                .HasPrincipalKey(q => new { q.Name, q.Version })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionnaireQuestionOption>(entity =>
        {
            entity.ConfigureVersionedBaseDbModelProperties();

            entity.Property(e => e.QuestionnaireQuestionName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionnaireQuestionVersion); // Nullable for "use latest"
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.Label).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.MediaUrl).HasMaxLength(1000);
            entity.Property(e => e.Score).HasPrecision(18, 4);
            entity.Property(e => e.Weight).HasPrecision(18, 4);
            entity.Property(e => e.Wa).HasPrecision(18, 4);

            entity.HasIndex(e => new { e.QuestionnaireQuestionName, e.QuestionnaireQuestionVersion, e.Order })
                .IsUnique()
                .HasDatabaseName("UX_QQOpt_QQName_QQVer_Order");
            entity.HasIndex(e => e.MediaFileId).IsUnique();

            // Ref: QuestionnaireQuestionOption.mediaFileId - File.id (1:1 per-table uniqueness)
            entity.HasOne(e => e.MediaFile)
                .WithMany()
                .HasForeignKey(e => e.MediaFileId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuestionnaireCandidateSubmission>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.JobApplicationStepId).IsRequired();
            entity.Property(e => e.QuestionnaireTemplateName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionnaireTemplateVersion).IsRequired();
            entity.Property(e => e.TemplateType)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();
            entity.Property(e => e.PersonalityResultJson);
            entity.Property(e => e.TotalScore).HasPrecision(18, 4);
            entity.Property(e => e.MaxScore).HasPrecision(18, 4);

            entity.HasIndex(e => e.JobApplicationStepId).IsUnique();
            entity.HasIndex(e => new { e.QuestionnaireTemplateName, e.QuestionnaireTemplateVersion });

            entity.HasOne(e => e.JobApplicationStep)
                .WithOne(jas => jas.QuestionnaireCandidateSubmission)
                .HasForeignKey<QuestionnaireCandidateSubmission>(e => e.JobApplicationStepId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.QuestionnaireTemplate)
                .WithMany()
                .HasForeignKey(e => new { e.QuestionnaireTemplateName, e.QuestionnaireTemplateVersion })
                .HasPrincipalKey(t => new { t.Name, t.Version })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Answers)
                .WithOne(a => a.QuestionnaireCandidateSubmission)
                .HasForeignKey(a => a.QuestionnaireCandidateSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionnaireCandidateSubmissionAnswer>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.QuestionnaireCandidateSubmissionId).IsRequired();
            entity.Property(e => e.QuestionnaireQuestionName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionnaireQuestionVersion).IsRequired();
            entity.Property(e => e.QuestionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();
            entity.Property(e => e.QuestionOrder).IsRequired();
            entity.Property(e => e.ScoreAwarded).HasPrecision(18, 4);
            entity.Property(e => e.WaSum).HasPrecision(18, 4);

            entity.HasIndex(e => new { e.QuestionnaireCandidateSubmissionId, e.QuestionnaireQuestionName, e.QuestionnaireQuestionVersion })
                .IsUnique()
                .HasDatabaseName("UX_QCSAns_Sub_QQName_QQVer");
            entity.HasIndex(e => new { e.QuestionnaireQuestionName, e.QuestionnaireQuestionVersion })
                .HasDatabaseName("IX_QCSAns_QQName_QQVer");

            entity.HasOne(e => e.QuestionnaireQuestion)
                .WithMany()
                .HasForeignKey(e => new { e.QuestionnaireQuestionName, e.QuestionnaireQuestionVersion })
                .HasPrincipalKey(q => new { q.Name, q.Version })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.SelectedOptions)
                .WithOne(o => o.QuestionnaireCandidateSubmissionAnswer)
                .HasForeignKey(o => o.QuestionnaireCandidateSubmissionAnswerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionnaireCandidateSubmissionAnswerOption>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.QuestionnaireCandidateSubmissionAnswerId).IsRequired();
            entity.Property(e => e.QuestionnaireQuestionOptionName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuestionnaireQuestionOptionVersion).IsRequired();
            entity.Property(e => e.Score).HasPrecision(18, 4);
            entity.Property(e => e.Wa).HasPrecision(18, 4);

            entity.HasIndex(e => new { e.QuestionnaireCandidateSubmissionAnswerId, e.QuestionnaireQuestionOptionName, e.QuestionnaireQuestionOptionVersion })
                .IsUnique()
                .HasDatabaseName("UX_QCSAnsOpt_Ans_OptName_OptVer");
            entity.HasIndex(e => new { e.QuestionnaireQuestionOptionName, e.QuestionnaireQuestionOptionVersion })
                .HasDatabaseName("IX_QCSAnsOpt_OptName_OptVer");

            entity.HasOne(e => e.QuestionnaireQuestionOption)
                .WithMany()
                .HasForeignKey(e => new { e.QuestionnaireQuestionOptionName, e.QuestionnaireQuestionOptionVersion })
                .HasPrincipalKey(o => new { o.Name, o.Version })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<KeyStrength>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.StrengthName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.KeyStrengths)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.StrengthName);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.SkillName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Proficiency).HasMaxLength(50);
            entity.Property(e => e.YearsExperience);
            entity.Property(e => e.Unit).HasMaxLength(50);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.Skills)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.SkillName);
        });

        modelBuilder.Entity<Experience>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Organization).HasMaxLength(200);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.StartDate);
            entity.Property(e => e.EndDate);
            entity.Property(e => e.Description);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.Experiences)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Organization);
            entity.HasIndex(e => e.StartDate);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Degree).HasMaxLength(200);
            entity.Property(e => e.Institution).HasMaxLength(200);
            entity.Property(e => e.FieldOfStudy).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.StartDate);
            entity.Property(e => e.EndDate);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.Educations)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Institution);
            entity.HasIndex(e => e.Degree);
        });

        modelBuilder.Entity<CvEvaluation>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.PromptCategory).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PromptVersion).IsRequired();
            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.ModelUsed).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ResponseJson).IsRequired().HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.CvEvaluations)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ref: CvEvaluation.fileId > File.id
            entity.HasOne(e => e.File)
                .WithMany()
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.FileId);
            entity.HasIndex(e => e.PromptCategory);
            entity.HasIndex(e => new { e.UserProfileId, e.FileId });
        });

        modelBuilder.Entity<AwardAchievement>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Issuer).HasMaxLength(200);
            entity.Property(e => e.Year);
            entity.Property(e => e.Description);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.AwardAchievements)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Title);
        });

        modelBuilder.Entity<CertificationLicense>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Issuer).HasMaxLength(200);
            entity.Property(e => e.DateIssued);
            entity.Property(e => e.ValidUntil);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.CertificationLicenses)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Name);
        });

        modelBuilder.Entity<Summary>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Text);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.Summaries)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
        });

        modelBuilder.Entity<Scoring>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.CvEvaluationId).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FixedCategory).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Score);
            entity.Property(e => e.Years);
            entity.Property(e => e.Level).HasMaxLength(50);

            entity.HasOne(e => e.CvEvaluation)
                .WithMany(cv => cv.Scorings)
                .HasForeignKey(e => e.CvEvaluationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.CvEvaluationId);
            entity.HasIndex(e => e.Category);
        });

        modelBuilder.Entity<VolunteerExtracurricular>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(200);
            entity.Property(e => e.Organization).HasMaxLength(200);
            entity.Property(e => e.StartDate);
            entity.Property(e => e.EndDate);
            entity.Property(e => e.Description);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.VolunteerExtracurriculars)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Organization);
            entity.HasIndex(e => e.StartDate);
        });

        modelBuilder.Entity<ProjectResearch>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();

            entity.Property(e => e.UserProfileId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Description);
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.TechnologiesUsed).HasMaxLength(500);
            entity.Property(e => e.Link).HasMaxLength(500);

            entity.HasOne(e => e.UserProfile)
                .WithMany(up => up.ProjectResearches)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Role);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ConfigureBaseDbModelProperties();
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.EntityType).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ParentCommentId);

            //composite index foṙ fast lookup
            entity.HasIndex(e => new { e.EntityId, e.EntityType });
            entity.HasIndex(e => e.ParentCommentId);
        });

        // Configure EntitySyncConfiguration entity (uses BasicBaseDbModel - no GDPR sync)
        modelBuilder.Entity<EntitySyncConfiguration>(entity =>
        {
            entity.ConfigureBasicBaseDbModelProperties();

            entity.HasKey(e => e.EntityTypeName);

            entity.Property(e => e.EntityTypeName)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.TableName)
                .HasMaxLength(128);

            entity.Property(e => e.DataClassification)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.SyncScope)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.LegalBasis)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.LegalBasisRef)
                .HasMaxLength(256);

            entity.Property(e => e.ProcessingPurpose)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.DependsOnEntities)
                .HasMaxLength(512);

            entity.Property(e => e.Notes)
                .HasMaxLength(1024);

            // Index for enabled entities
            entity.HasIndex(e => e.IsEnabled);
        });
    }
}
