
USE [sql-recruiter-test];
GO

-- Create all tables with UNIQUEIDENTIFIER primary keys and PascalCase field names
CREATE TABLE UserProfiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ResumeUrl NVARCHAR(500) NULL,
    Name NVARCHAR(255) NULL,
    Email NVARCHAR(255) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    Age INT NULL,
    Nationality NVARCHAR(100) NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    Bio NTEXT NULL,
    JobTypePreferences NVARCHAR(MAX) NULL, -- JSON data
    OpenToRelocation BIT NULL,
    RemotePreferences NVARCHAR(MAX) NULL, -- JSON data
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Files (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Container NVARCHAR(255) NULL,
    FilePath NVARCHAR(500) NULL,
    Extension NVARCHAR(10) NULL,
    MbSize INT NULL,
    StorageAccountName NVARCHAR(MAX) NULL, -- JSON data
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Candidates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CvFileId UNIQUEIDENTIFIER NULL,
    UserProfileId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CvFileId) REFERENCES Files(Id),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE Prompts (
    Name NVARCHAR(255) NOT NULL,
    Category NVARCHAR(100) NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Version INT NOT NULL,
    Locale NVARCHAR(50) NULL,
    Tags NVARCHAR(MAX) NULL, 
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (Name, Version) 
);

CREATE TABLE CVEvaluations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    PromptCategory NVARCHAR(255) NULL,
    PromptVersion INT NULL,
    FileId UNIQUEIDENTIFIER NOT NULL,
    ModelUsed NVARCHAR(100) NOT NULL,
    ResponseJson NVARCHAR(MAX) NOT NULL, -- JSON data
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id),
    FOREIGN KEY (FileId) REFERENCES Files(Id)
);

CREATE TABLE Experiences (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NULL,
    Organization NVARCHAR(255) NULL,
    Industry NVARCHAR(255) NULL,
    Location NVARCHAR(255) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Description NTEXT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE Educations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Degree NVARCHAR(255) NULL,
    Institution NVARCHAR(255) NULL,
    FieldOfStudy NVARCHAR(255) NULL,
    Location NVARCHAR(255) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE Skills (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Category NVARCHAR(255) NULL,
    SkillName NVARCHAR(255) NULL,
    Proficiency NVARCHAR(100) NULL,
    YearsExperience INT NULL,
    Unit NVARCHAR(50) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE ProjectsResearch (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NULL,
    Description NTEXT NULL,
    Role NVARCHAR(255) NULL,
    TechnologiesUsed NVARCHAR(500) NULL,
    Link NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE CertificationsLicenses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(255) NULL,
    Issuer NVARCHAR(255) NULL,
    DateIssued DATE NULL,
    ValidUntil DATE NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE AwardsAchievements (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NULL,
    Issuer NVARCHAR(255) NULL,
    Year INT NULL,
    Description NTEXT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE VolunteerExtracurricular (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Role NVARCHAR(255) NULL,
    Organization NVARCHAR(255) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Description NTEXT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE Scorings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CVEvaluationId UNIQUEIDENTIFIER NOT NULL,
    Category NVARCHAR(255) NULL,
    Score INT NULL,
    Years INT NULL,
    Level NVARCHAR(100) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CVEvaluationId) REFERENCES CVEvaluations(Id)
);

CREATE TABLE Summaries (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(100) NULL,
    Text NTEXT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

CREATE TABLE KeyStrengths (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserProfileId UNIQUEIDENTIFIER NOT NULL,
    StrengthName NVARCHAR(255) NULL,
    Description NTEXT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id)
);

-- Seed the Prompts table with CV extraction prompt
INSERT INTO Prompts (Name, Version, Category, Content, Locale, Tags)
VALUES (
    'cv_extraction',
    1,
    'cv_extraction',
    'You are a helpful assistant specialized in extracting structured data from resumes and CVs. 
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
    '["cv", "resume", "extraction", "analysis", "professional"]'
);

PRINT 'Database schema created successfully with all tables and seeded prompts.';