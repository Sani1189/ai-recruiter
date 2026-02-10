USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'CvExtractionDB')
BEGIN
    ALTER DATABASE CvExtractionDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CvExtractionDB;
    PRINT 'Database CvExtractionDB dropped successfully.';
END
GO

CREATE DATABASE CvExtractionDB;
GO

USE CvExtractionDB;
GO

------------------------------------------------------------
-- Base fields for all tables except Prompts:
-- Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
-- CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
-- UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
-- CreatedBy NVARCHAR(100) NULL,
-- UpdatedBy NVARCHAR(100) NULL,
-- IsDeleted BIT NOT NULL DEFAULT 0,
-- RowVersion ROWVERSION NOT NULL
------------------------------------------------------------

-- ===========================================================
-- UserProfiles
-- ===========================================================
CREATE TABLE UserProfiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Nationality NVARCHAR(100) NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    ResumeUrl NVARCHAR(500) NULL,
    JobTypePreferences NVARCHAR(MAX) NULL,
    RemotePreferences NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    CONSTRAINT CK_UserProfiles_Name_NotEmpty CHECK (LEN(Name) > 0)
);
CREATE UNIQUE INDEX IX_UserProfiles_Email ON UserProfiles(Email);
CREATE INDEX IX_UserProfiles_Name ON UserProfiles(Name);

-- ===========================================================
-- Files
-- ===========================================================
CREATE TABLE Files (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Container NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    Extension NVARCHAR(10) NOT NULL,
    MbSize INT NOT NULL,
    StorageAccountName NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE INDEX IX_Files_Container ON Files(Container);
CREATE INDEX IX_Files_Extension ON Files(Extension);

-- ===========================================================
-- Candidates
-- ===========================================================
CREATE TABLE Candidates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    CvFileId UNIQUEIDENTIFIER NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id),
    FOREIGN KEY (CvFileId) REFERENCES Files(Id)
);
CREATE UNIQUE INDEX IX_Candidates_UserProfileId ON Candidates(UserProfileId);
CREATE INDEX IX_Candidates_CvFileId ON Candidates(CvFileId);

-- ===========================================================
-- Prompts (no audit fields)
-- ===========================================================
CREATE TABLE Prompts (
    Name NVARCHAR(255) NOT NULL,
    Version INT NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Locale NVARCHAR(10) NULL,
    Tags NVARCHAR(MAX) NULL,
    CONSTRAINT PK_Prompts PRIMARY KEY (Name, Version)
);
CREATE UNIQUE INDEX UQ_Prompts_Name_Version ON Prompts(Name, Version);
CREATE INDEX IX_Prompts_Category ON Prompts(Category);
CREATE INDEX IX_Prompts_Locale ON Prompts(Locale);

-- ===========================================================
-- CvEvaluations
-- ===========================================================
CREATE TABLE CVEvaluations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    PromptCategory NVARCHAR(255) NOT NULL,
    PromptVersion INT NOT NULL,
    FileId UNIQUEIDENTIFIER NOT NULL,
    ModelUsed NVARCHAR(100) NOT NULL,
    ResponseJson NVARCHAR(MAX) NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id),
    FOREIGN KEY (FileId) REFERENCES Files(Id)
);
CREATE INDEX IX_CVEvaluations_UserProfileId ON CVEvaluations(UserProfileId);
CREATE INDEX IX_CVEvaluations_FileId ON CVEvaluations(FileId);
CREATE INDEX IX_CVEvaluations_PromptCategory ON CVEvaluations(PromptCategory);
CREATE INDEX IX_CVEvaluations_UserProfileId_FileId ON CVEvaluations(UserProfileId, FileId);

-- ===========================================================
-- KeyStrengths
-- ===========================================================
CREATE TABLE KeyStrengths (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    StrengthName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_KeyStrengths_UserProfileId ON KeyStrengths(UserProfileId);
CREATE INDEX IX_KeyStrengths_StrengthName ON KeyStrengths(StrengthName);

-- ===========================================================
-- Skills
-- ===========================================================
CREATE TABLE Skills (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Category NVARCHAR(100) NULL,
    SkillName NVARCHAR(200) NOT NULL,
    Proficiency NVARCHAR(50) NULL,
    YearsExperience INT NULL,
    Unit NVARCHAR(50) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_Skills_UserProfileId ON Skills(UserProfileId);
CREATE INDEX IX_Skills_Category ON Skills(Category);
CREATE INDEX IX_Skills_SkillName ON Skills(SkillName);

-- ===========================================================
-- Scorings
-- ===========================================================
CREATE TABLE Scorings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Score INT NULL,
    Years INT NULL,
    Level NVARCHAR(50) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_Scorings_UserProfileId ON Scorings(UserProfileId);
CREATE INDEX IX_Scorings_Category ON Scorings(Category);

-- ===========================================================
-- Educations
-- ===========================================================
CREATE TABLE Educations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Degree NVARCHAR(200) NULL,
    Institution NVARCHAR(200) NULL,
    FieldOfStudy NVARCHAR(200) NULL,
    Location NVARCHAR(200) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_Educations_UserProfileId ON Educations(UserProfileId);
CREATE INDEX IX_Educations_Institution ON Educations(Institution);
CREATE INDEX IX_Educations_Degree ON Educations(Degree);

-- ===========================================================
-- Experiences
-- ===========================================================
CREATE TABLE Experiences (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NULL,
    Organization NVARCHAR(200) NULL,
    Industry NVARCHAR(100) NULL,
    Location NVARCHAR(200) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Description NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_Experiences_UserProfileId ON Experiences(UserProfileId);
CREATE INDEX IX_Experiences_Organization ON Experiences(Organization);
CREATE INDEX IX_Experiences_StartDate ON Experiences(StartDate);

-- ===========================================================
-- ProjectsResearch
-- ===========================================================
CREATE TABLE ProjectsResearch (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NULL,
    Description NVARCHAR(MAX) NULL,
    Role NVARCHAR(100) NULL,
    TechnologiesUsed NVARCHAR(500) NULL,
    Link NVARCHAR(500) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_ProjectsResearch_UserProfileId ON ProjectsResearch(UserProfileId);
CREATE INDEX IX_ProjectsResearch_Title ON ProjectsResearch(Title);

-- ===========================================================
-- CertificationsLicenses
-- ===========================================================
CREATE TABLE CertificationsLicenses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Issuer NVARCHAR(200) NULL,
    DateIssued DATE NULL,
    ValidUntil DATE NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_CertificationsLicenses_UserProfileId ON CertificationsLicenses(UserProfileId);
CREATE INDEX IX_CertificationsLicenses_Name ON CertificationsLicenses(Name);

-- ===========================================================
-- AwardsAchievements
-- ===========================================================
CREATE TABLE AwardsAchievements (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Issuer NVARCHAR(200) NULL,
    Year INT NULL,
    Description NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_AwardsAchievements_UserProfileId ON AwardsAchievements(UserProfileId);
CREATE INDEX IX_AwardsAchievements_Title ON AwardsAchievements(Title);

-- ===========================================================
-- VolunteerExtracurricular
-- ===========================================================
CREATE TABLE VolunteerExtracurricular (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Role NVARCHAR(200) NULL,
    Organization NVARCHAR(200) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Description NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_VolunteerExtracurricular_UserProfileId ON VolunteerExtracurricular(UserProfileId);
CREATE INDEX IX_VolunteerExtracurricular_Organization ON VolunteerExtracurricular(Organization);

-- ===========================================================
-- Summaries
-- ===========================================================
CREATE TABLE Summaries (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(100) NOT NULL,
    Text NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,

    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);
CREATE INDEX IX_Summaries_UserProfileId ON Summaries(UserProfileId);

------------------------------------------------------------
-- Seed default prompt
------------------------------------------------------------
INSERT INTO Prompts (Name, Version, Category, Content, Locale, Tags)
VALUES (
    'cv_extraction',
    1,
    'cv_extraction',
    N'You are a helpful assistant specialized in extracting structured data from resumes and CVs. 
Your response must be in JSON format only. Analyze the provided CV/resume text and extract all available information.
If any information is missing or not available, use null values. Be thorough and extract as much relevant information as possible.

Please analyze the CV/resume text and extract all available information in the following categories:

1. **Personal Information**: Name, email, phone number, age, nationality, bio, job preferences, relocation willingness, remote work preferences
2. **Professional Experience**: Job titles, organizations, industries, locations, employment dates, job descriptions
3. **Education**: Degrees, institutions, fields of study, locations, academic dates
4. **Skills**: Technical and soft skills with categories, proficiency levels, and years of experience
5. **Projects & Research**: Project titles, descriptions, roles, technologies used, links
6. **Certifications & Licenses**: Names, issuers, issue dates, expiration dates
7. **Awards & Achievements**: Titles, issuers, years, descriptions
8. **Volunteer & Extracurricular**: Roles, organizations, dates, descriptions
9. **Scoring**: Evaluate skills and experience with scores (1-10), years of experience, and proficiency levels
10. **Summaries**: Create professional summaries and career highlights
11. **Key Strengths**: Identify and describe main professional strengths

**Important Guidelines:**
- For date fields, use YYYY-MM-DD format
- For score and yearsExperience fields, ensure values are integers only
- Extract information comprehensively and accurately
- Use null for missing information
- Maintain professional tone and accuracy
- Focus on relevant professional information',
    'en-US',
    N'["cv", "resume", "extraction", "analysis", "professional"]'
);

PRINT '✅ Database schema created successfully with all tables and seeded prompts.';
GO
