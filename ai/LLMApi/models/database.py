"""
SQLAlchemy models matching C# EF Core configuration and Models.txt schema.
- BaseDbModel: Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, RowVersion
- Prompt: VersionedBaseDbModel → composite PK, no audit
- Scorings → CvEvaluationId (linked to CvEvaluation per Models.txt line 214)
"""

from __future__ import annotations

from typing import Tuple

from sqlalchemy import (
    Column, String, Integer, Boolean, Text, Date, DateTime, JSON, ForeignKey, Float,
    UniqueConstraint, CheckConstraint, Index, text, event, TypeDecorator
)
from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER, ROWVERSION
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func

Base = declarative_base()

class CsvStringList(TypeDecorator):
    """
    Matches EF Core conversion:
      v => string.Join(',', v)
      v => v.Split(',', RemoveEmptyEntries)
    """

    impl = Text
    cache_ok = True

    def process_bind_param(self, value, dialect):
        if not value:
            return ""
        if isinstance(value, str):
            return value
        return ",".join([str(x).strip() for x in value if str(x).strip()])

    def process_result_value(self, value, dialect):
        if not value:
            return []
        raw = value if isinstance(value, str) else str(value)
        return [x.strip() for x in raw.split(",") if x.strip()]


# ================================================================
# BaseDbModel Mixin (for all entities except Prompt)
# ================================================================
class BaseDbModel:
    """Common audit + concurrency fields from C# BaseDbModel"""
    Id = Column(
        UNIQUEIDENTIFIER,
        primary_key=True,
        server_default=func.newid()
    )
    CreatedAt = Column(
        DateTime,
        nullable=False,
        server_default=func.getutcdate()
    )
    UpdatedAt = Column(
        DateTime,
        nullable=False,
        server_default=func.getutcdate(),
        onupdate=func.getutcdate()
    )
    CreatedBy = Column(String(100), nullable=True)
    UpdatedBy = Column(String(100), nullable=True)
    IsDeleted = Column(Boolean, nullable=False, server_default=text("0"))
    # RowVersion is automatically created and maintained by SQL Server, so we don't include it in SQLAlchemy


# ================================================================
# UserProfile
# ================================================================
class UserProfile(Base, BaseDbModel):
    __tablename__ = "UserProfiles"

    Name = Column(String(255), nullable=False)
    Email = Column(String(255), nullable=False)
    PhoneNumber = Column(String(20), nullable=True)
    Nationality = Column(String(100), nullable=True)
    ProfilePictureUrl = Column(String(500), nullable=True)
    ResumeUrl = Column(String(500), nullable=True)
    JobTypePreferences = Column(String(2000), nullable=False, server_default=text("''"))
    RemotePreferences = Column(String(2000), nullable=False, server_default=text("''"))
    Roles = Column(String(2000), nullable=False, server_default=text("''"))
    Age = Column(Integer, nullable=True)
    Bio = Column(Text, nullable=True)
    OpenToRelocation = Column(Boolean, nullable=False, server_default=text("0"))  # Default to False

    # Relationships
    Candidate = relationship("Candidate", back_populates="UserProfile", uselist=False)
    KeyStrengths = relationship("KeyStrength", back_populates="UserProfile")
    Skills = relationship("Skill", back_populates="UserProfile")
    Experiences = relationship("Experience", back_populates="UserProfile")
    Educations = relationship("Education", back_populates="UserProfile")
    CvEvaluations = relationship("CvEvaluation", back_populates="UserProfile")
    AwardsAchievements = relationship("AwardsAchievements", back_populates="UserProfile")
    CertificationsLicenses = relationship("CertificationsLicenses", back_populates="UserProfile")
    Summaries = relationship("Summary", back_populates="UserProfile")
    VolunteerExtracurricular = relationship("VolunteerExtracurricular", back_populates="UserProfile")
    ProjectsResearch = relationship("ProjectsResearch", back_populates="UserProfile")

    __table_args__: Tuple[Index, Index, CheckConstraint] = (
        Index('IX_UserProfiles_Email', 'Email', unique=True),
        Index('IX_UserProfiles_Name', 'Name'),
        CheckConstraint("LEN(Name) > 0", name="CK_UserProfiles_Name_NotEmpty"),
    )


# ================================================================
# Candidate (1:1 with UserProfile)
# ================================================================
class Candidate(Base, BaseDbModel):
    __tablename__ = "Candidates"

    CandidateId = Column(String(20), nullable=False)
    CvFileId = Column(UNIQUEIDENTIFIER, ForeignKey('Files.Id'), nullable=True)
    UserId = Column(
        UNIQUEIDENTIFIER,
        ForeignKey('UserProfiles.Id'),
        nullable=False,
        unique=True
    )

    UserProfile = relationship("UserProfile", back_populates="Candidate")
    CvFile = relationship("File")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_Candidates_UserId', 'UserId', unique=True),
        Index('IX_Candidates_CvFileId', 'CvFileId'),
    )


# ================================================================
# File
# ================================================================
class File(Base, BaseDbModel):
    __tablename__ = "Files"

    Container = Column(String(255), nullable=False)
    FolderPath = Column(String(500), nullable=True)
    FilePath = Column(String(500), nullable=False)
    Extension = Column(String(10), nullable=False)
    MbSize = Column(Integer, nullable=False)
    StorageAccountName = Column(String(500), nullable=True, server_default=text("''"))

    __table_args__: Tuple[Index, Index] = (
        Index('IX_Files_Container', 'Container'),
        Index('IX_Files_Extension', 'Extension'),
    )
    
    def get_full_blob_path(self) -> str:
        """Helper to get full blob path: FolderPath/FilePath (matching C# pattern)"""
        if self.FolderPath:
            return f"{self.FolderPath}/{self.FilePath}"
        return self.FilePath


# ================================================================
# Prompt (VersionedBaseDbModel → no BaseDbModel)
# ================================================================
class Prompt(Base):
    __tablename__ = "Prompts"

    Name = Column(String(255), primary_key=True)
    Version = Column(Integer, primary_key=True)
    Category = Column(String(100), nullable=False)
    Content = Column(Text, nullable=False)
    Locale = Column(String(10), nullable=True)
    # Matches C# EF Core conversion: stored as comma-separated nvarchar(max)
    Tags = Column(CsvStringList, nullable=False, server_default=text("''"))  # List[str]

    __table_args__: Tuple[UniqueConstraint, Index, Index] = (
        UniqueConstraint('Name', 'Version', name='UQ_Prompts_Name_Version'),
        Index('IX_Prompts_Category', 'Category'),
        Index('IX_Prompts_Locale', 'Locale'),
    )


# ================================================================
# CvEvaluation
# ================================================================
class CvEvaluation(Base, BaseDbModel):
    __tablename__ = "CVEvaluations"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    PromptCategory = Column(String(255), nullable=False)
    PromptVersion = Column(Integer, nullable=False)
    FileId = Column(UNIQUEIDENTIFIER, ForeignKey('Files.Id'), nullable=False)
    ModelUsed = Column(String(100), nullable=False)
    ResponseJson = Column(Text, nullable=False)

    UserProfile = relationship("UserProfile", back_populates="CvEvaluations")
    File = relationship("File")
    Scorings = relationship("Scoring", back_populates="CvEvaluation")

    __table_args__: Tuple[Index, Index, Index, Index] = (
        Index('IX_CVEvaluations_UserProfileId', 'UserProfileId'),
        Index('IX_CVEvaluations_FileId', 'FileId'),
        Index('IX_CVEvaluations_PromptCategory', 'PromptCategory'),
        Index('IX_CVEvaluations_UserProfileId_FileId', 'UserProfileId', 'FileId'),
    )


# ================================================================
# KeyStrength
# ================================================================
class KeyStrength(Base, BaseDbModel):
    __tablename__ = "KeyStrengths"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    StrengthName = Column(String(200), nullable=False)
    Description = Column(Text, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="KeyStrengths")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_KeyStrengths_UserProfileId', 'UserProfileId'),
        Index('IX_KeyStrengths_StrengthName', 'StrengthName'),
    )


# ================================================================
# Skill
# ================================================================
class Skill(Base, BaseDbModel):
    __tablename__ = "Skills"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Category = Column(String(100), nullable=True)
    SkillName = Column(String(200), nullable=False)
    Proficiency = Column(String(50), nullable=True)
    YearsExperience = Column(Integer, nullable=True)
    Unit = Column(String(50), nullable=True)

    UserProfile = relationship("UserProfile", back_populates="Skills")

    __table_args__: Tuple[Index, Index, Index] = (
        Index('IX_Skills_UserProfileId', 'UserProfileId'),
        Index('IX_Skills_Category', 'Category'),
        Index('IX_Skills_SkillName', 'SkillName'),
    )


# ================================================================
# Experience
# ================================================================
class Experience(Base, BaseDbModel):
    __tablename__ = "Experiences"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Title = Column(String(200), nullable=True)
    Organization = Column(String(200), nullable=True)
    Industry = Column(String(100), nullable=True)
    Location = Column(String(200), nullable=True)
    StartDate = Column(Date, nullable=True)
    EndDate = Column(Date, nullable=True)
    Description = Column(Text, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="Experiences")

    __table_args__: Tuple[Index, Index, Index] = (
        Index('IX_Experiences_UserProfileId', 'UserProfileId'),
        Index('IX_Experiences_Organization', 'Organization'),
        Index('IX_Experiences_StartDate', 'StartDate'),
    )


# ================================================================
# Education
# ================================================================
class Education(Base, BaseDbModel):
    __tablename__ = "Educations"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Degree = Column(String(200), nullable=True)
    Institution = Column(String(200), nullable=True)
    FieldOfStudy = Column(String(200), nullable=True)
    Location = Column(String(200), nullable=True)
    StartDate = Column(Date, nullable=True)
    EndDate = Column(Date, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="Educations")

    __table_args__: Tuple[Index, Index, Index] = (
        Index('IX_Educations_UserProfileId', 'UserProfileId'),
        Index('IX_Educations_Institution', 'Institution'),
        Index('IX_Educations_Degree', 'Degree'),
    )


# ================================================================
# ProjectsResearch
# ================================================================
class ProjectsResearch(Base, BaseDbModel):
    __tablename__ = "ProjectsResearch"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Title = Column(String(200), nullable=True)
    Description = Column(Text, nullable=True)
    Role = Column(String(100), nullable=True)
    TechnologiesUsed = Column(String(500), nullable=True)
    Link = Column(String(500), nullable=True)

    UserProfile = relationship("UserProfile", back_populates="ProjectsResearch")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_ProjectsResearch_UserProfileId', 'UserProfileId'),
        Index('IX_ProjectsResearch_Title', 'Title'),
    )


# ================================================================
# CertificationsLicenses
# ================================================================
class CertificationsLicenses(Base, BaseDbModel):
    __tablename__ = "CertificationsLicenses"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Name = Column(String(200), nullable=False)
    Issuer = Column(String(200), nullable=True)
    DateIssued = Column(Date, nullable=True)
    ValidUntil = Column(Date, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="CertificationsLicenses")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_CertificationsLicenses_UserProfileId', 'UserProfileId'),
        Index('IX_CertificationsLicenses_Name', 'Name'),
    )


# ================================================================
# AwardsAchievements
# ================================================================
class AwardsAchievements(Base, BaseDbModel):
    __tablename__ = "AwardsAchievements"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Title = Column(String(200), nullable=False)
    Issuer = Column(String(200), nullable=True)
    Year = Column(Integer, nullable=True)
    Description = Column(Text, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="AwardsAchievements")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_AwardsAchievements_UserProfileId', 'UserProfileId'),
        Index('IX_AwardsAchievements_Title', 'Title'),
    )


# ================================================================
# VolunteerExtracurricular
# ================================================================
class VolunteerExtracurricular(Base, BaseDbModel):
    __tablename__ = "VolunteerExtracurricular"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Role = Column(String(200), nullable=True)
    Organization = Column(String(200), nullable=True)
    StartDate = Column(Date, nullable=True)
    EndDate = Column(Date, nullable=True)
    Description = Column(Text, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="VolunteerExtracurricular")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_VolunteerExtracurricular_UserProfileId', 'UserProfileId'),
        Index('IX_VolunteerExtracurricular_Organization', 'Organization'),
    )


# ================================================================
# Summary
# ================================================================
class Summary(Base, BaseDbModel):
    __tablename__ = "Summaries"

    UserProfileId = Column(UNIQUEIDENTIFIER, ForeignKey('UserProfiles.Id'), nullable=False)
    Type = Column(String(100), nullable=False)
    Text = Column(Text, nullable=True)

    UserProfile = relationship("UserProfile", back_populates="Summaries")

    __table_args__: Tuple[Index] = (
        Index('IX_Summaries_UserProfileId', 'UserProfileId'),
    )


# ================================================================
# Scoring (uses CvEvaluationId, linked to CvEvaluation per Models.txt)
# ================================================================
class Scoring(Base, BaseDbModel):
    __tablename__ = "Scorings"

    CvEvaluationId = Column(UNIQUEIDENTIFIER, ForeignKey('CVEvaluations.Id'), nullable=False)
    Category = Column(String(100), nullable=False)
    FixedCategory = Column(String(100), nullable=False)
    Score = Column(Integer, nullable=True)
    Years = Column(Integer, nullable=True)
    Level = Column(String(50), nullable=True)

    CvEvaluation = relationship("CvEvaluation", back_populates="Scorings")

    __table_args__: Tuple[Index, Index] = (
        Index('IX_Scorings_CvEvaluationId', 'CvEvaluationId'),
        Index('IX_Scorings_Category', 'Category'),
    )

# ================================================================
# Scores (Interview Scores)
# ================================================================
class Score(Base, BaseDbModel):
    __tablename__ = "Scores"

    English = Column(Integer, nullable=False)
    Technical = Column(Integer, nullable=False)
    Communication = Column(Integer, nullable=False)
    ProblemSolving = Column(Integer, nullable=False)

    Average = Column(Float, nullable=False)

    InterviewId = Column(UNIQUEIDENTIFIER, ForeignKey('Interviews.Id'), nullable=False)
    
    __table_args__: Tuple[Index] = (
        Index('IX_Scores_InterviewId', 'InterviewId'),
    )

# ================================================================
# Interview
# ================================================================
class Interview(Base, BaseDbModel):
    __tablename__ = "Interviews"

    JobApplicationStepId = Column(UNIQUEIDENTIFIER, ForeignKey("JobApplicationSteps.Id"), nullable=False)
    InterviewAudioUrl = Column(String(1000), nullable=True)
    InterviewConfigurationName = Column(String(255), nullable=False)
    InterviewConfigurationVersion = Column(String(50), nullable=False)
    TranscriptUrl = Column(String(1000), nullable=True)
    CompletedAt = Column(DateTime, nullable=True)
    Duration = Column(Float, nullable=True)  # store duration in seconds
    InstructionPromptName = Column(String(255), nullable=False)
    InstructionPromptVersion = Column(String(50), nullable=False)
    PersonalityPromptName = Column(String(255), nullable=False)
    PersonalityPromptVersion = Column(String(50), nullable=False)
    QuestionsPromptName = Column(String(255), nullable=False)
    QuestionsPromptVersion = Column(String(50), nullable=False)
    StartedAt = Column(DateTime, nullable=True)

    __table_args__: Tuple[Index, Index] = (
        Index('IX_Interviews_JobApplicationStepId', 'JobApplicationStepId'),
        Index('IX_Interviews_TranscriptUrl', 'TranscriptUrl')
    )