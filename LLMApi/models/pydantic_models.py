# models/pydantic_models.py
from __future__ import annotations

from pydantic import BaseModel, Field, field_validator, ConfigDict
from typing import List, Literal
from datetime import date
import uuid

# ================================
# Core Data Models (match C#)
# ================================
class UserProfileData(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    ResumeUrl: str | None = None
    Name: str | None = None
    Email: str | None = None
    PhoneNumber: str | None = None
    Age: int | None = None
    Nationality: str | None = None
    ProfilePictureUrl: str | None = None
    Bio: str | None = None
    JobTypePreferences: List[str] = Field(default_factory=list)
    OpenToRelocation: bool | None = None
    RemotePreferences: List[str] = Field(default_factory=list)
    Roles: List[str] = Field(default_factory=list)

    @field_validator("JobTypePreferences", "RemotePreferences", mode="before")
    @classmethod
    def ensure_list(cls, v):
        if v is None:
            return []
        if isinstance(v, str):
            return [v.strip()] if v.strip() else []
        if isinstance(v, list):
            return [item.strip() for item in v if item]
        return []


class CandidateData(BaseModel):
    CvFileId: str | None = None
    UserProfileId: str | None = None


class ExperienceData(BaseModel):
    Title: str | None = None
    Organization: str | None = None
    Industry: str | None = None
    Location: str | None = None
    StartDate: date | None = None
    EndDate: date | None = None
    Description: str | None = None


class EducationData(BaseModel):
    Degree: str | None = None
    Institution: str | None = None
    FieldOfStudy: str | None = None
    Location: str | None = None
    StartDate: date | None = None
    EndDate: date | None = None


class SkillData(BaseModel):
    Category: str | None = None
    SkillName: str | None = None
    Proficiency: str | None = None
    YearsExperience: int | None = None
    Unit: str | None = None


class ProjectData(BaseModel):
    Title: str | None = None
    Description: str | None = None
    Role: str | None = None
    TechnologiesUsed: str | None = None
    Link: str | None = None


class CertificationData(BaseModel):
    Name: str | None = None
    Issuer: str | None = None
    DateIssued: date | None = None
    ValidUntil: date | None = None


class AwardData(BaseModel):
    Title: str | None = None
    Issuer: str | None = None
    Year: int | None = None
    Description: str | None = None


class VolunteerData(BaseModel):
    Role: str | None = None
    Organization: str | None = None
    StartDate: date | None = None
    EndDate: date | None = None
    Description: str | None = None


class ScoringData(BaseModel):
    Category: str | None = None
    FixedCategory: str | None = None
    Score: int | None = Field(None, ge=1, le=10)
    Years: int | None = None
    Level: Literal["Junior", "Mid", "Senior", "Expert"] | None = None


class SummaryData(BaseModel):
    Type: Literal["Positives", "Negatives", "Overall", "Weaknesses"] | None = None
    Text: str | None = None


class KeyStrengthData(BaseModel):
    StrengthName: str | None = None
    Description: str | None = None


# ================================
# Final Response (for OpenAI json_schema)
# ================================
class CVExtractionResponse(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    UserProfile: UserProfileData
    Candidate: CandidateData
    Experience: List[ExperienceData] = Field(default_factory=list)
    Education: List[EducationData] = Field(default_factory=list)
    Skills: List[SkillData] = Field(default_factory=list)
    ProjectsResearch: List[ProjectData] = Field(default_factory=list)
    CertificationsLicenses: List[CertificationData] = Field(default_factory=list)
    AwardsAchievements: List[AwardData] = Field(default_factory=list)
    VolunteerExtracurricular: List[VolunteerData] = Field(default_factory=list)
    Scoring: List[ScoringData] = Field(default_factory=list)
    Summaries: List[SummaryData] = Field(default_factory=list)
    KeyStrengths: List[KeyStrengthData] = Field(default_factory=list)


# ================================
# Request / Response DTOs
# ================================
class CVUploadRequest(BaseModel):
    promptCategory: Literal["cv_extraction"] = "cv_extraction"
    promptVersion: int = Field(1, ge=1)

class CVProcessingStatus(BaseModel):
    FileId: str
    UserId: str


class CVUploadResponse(BaseModel):
    fileId: str
    userId: str | None = None
    message: str
    status: str

# ================================
# ElevenLabs Webhook DTOs
# ================================
class TranscriptItem(BaseModel):
    role: Literal["agent", "user"]
    message: str
    time_in_call_secs: int

class TranscriptFileReference(BaseModel):
    container: str
    blob_path: str
    conversation_id: str
    job_application_id: str
    job_post_name: str
    job_post_version: int
    storage_account: str

# ================================
# Interview Scoring Models
# ===============================
TOTAL_NUMBER_OF_SCORING_CATEGORIES = 4
DEFAULT_SCORING_WEIGHT = 100 // TOTAL_NUMBER_OF_SCORING_CATEGORIES

class ScoringCriteria(BaseModel):
    Technical: int = Field(default=DEFAULT_SCORING_WEIGHT, ge=0, le=100, description="Weight for technical skills")
    Communication: int = Field(default=DEFAULT_SCORING_WEIGHT, ge=0, le=100, description="Weight for communication")
    ProblemSolving: int = Field(default=DEFAULT_SCORING_WEIGHT, ge=0, le=100, description="Weight for problem solving")
    English: int = Field(default=DEFAULT_SCORING_WEIGHT, ge=0, le=100, description="Weight for English proficiency")

class InterviewScore(BaseModel):
    Technical: int = Field(..., ge=0, le=100, description="Score for technical skills from 0-100")
    Communication: int = Field(..., ge=0, le=100, description="Score for communication skills from 0-100")
    ProblemSolving: int = Field(..., ge=0, le=100, description="Score for problem solving skills from 0-100")
    English: int = Field(..., ge=0, le=100, description="Score for English proficiency from 0-100")