from fastapi import APIRouter, UploadFile, File, Form, HTTPException, Depends
from sqlalchemy.orm import Session
from sqlalchemy import func
from database import get_db
from services.azure_storage import azure_storage
from services.service_bus import service_bus
from models.pydantic_models import CVUploadResponse, CVProcessingStatus
from models.database import UserProfile, CVEvaluation, Prompt, Candidate, File as FileModel
from config import settings
import uuid
import logging
from typing import List, Optional

logger = logging.getLogger(__name__)
router = APIRouter()

@router.post("/upload-cv", response_model=CVUploadResponse)
async def upload_cv(
    file: UploadFile = File(...),
    prompt_name: Optional[str] = Form(default=None),
    prompt_category: str = Form(default="cv_extraction"),
    prompt_version: int = Form(default=1),
    userProfileId: Optional[str] = Form(default=None),
    db: Session = Depends(get_db),
):
    """Upload CV to blob storage and send to processing queue - auto-generates user ID"""
    try:
        # Validate file
        if not file.filename:
            raise HTTPException(status_code=400, detail="No file provided")
        
        file_extension = file.filename.split('.')[-1].lower()
        if file_extension not in settings.allowed_extensions_list:
            raise HTTPException(
                status_code=400, 
                detail=f"File type not allowed. Allowed types: {settings.allowed_file_extensions}"
            )
        
        # Check file size
        file_content = await file.read()
        file_size_mb = len(file_content) / (1024 * 1024)
        if file_size_mb > settings.max_file_size_mb:
            raise HTTPException(
                status_code=400,
                detail=f"File too large. Maximum size: {settings.max_file_size_mb}MB"
            )
        
        # Get or create user profile
        if userProfileId:
            try:
                uuid.UUID(userProfileId)
            except ValueError:
                raise HTTPException(status_code=400, detail=f"Invalid userProfileId format: {userProfileId}")
            user_id = userProfileId
        else:
            user_id = str(uuid.uuid4())
        
        # Check if user exists, create if not (without Email - unique constraint)
        user_profile = db.query(UserProfile).filter(UserProfile.Id == user_id).first()
        if not user_profile:
            user_profile = UserProfile(Id=user_id, Name="", Email="")
            db.add(user_profile)
            db.flush()
        
        # Generate file ID
        file_id = str(uuid.uuid4())
        
        # Upload to blob storage
        blob_path = await azure_storage.upload_file(
            file_content, user_id, file_id, file_extension
        )
        
        file_record = FileModel(
            Id=file_id,
            Container="cvs",
            FilePath=blob_path,
            Extension=file_extension,
            MbSize=int(file_size_mb),
            StorageAccountName=["default"]
        )
        db.add(file_record)
        
        candidate = Candidate(
            Id=str(uuid.uuid4()),
            UserId=user_id,
            CandidateId="",  # Will be set by C# backend if needed
            CvFileId=file_id
        )
        db.add(candidate)
        db.commit()
        
        message_data = {
            "userId": user_id,
            "fileId": file_id,
            "fileExtension": file_extension,
            "promptName": prompt_name,
            "promptCategory": prompt_category,
            "promptVersion": prompt_version,
            "blobPath": blob_path
        }
        
        queue_success = await service_bus.send_cv_processing_message(message_data)
        
        if queue_success:
            logger.info(f"CV uploaded and queued: {file_id} for user: {user_id}")
            return CVUploadResponse(
                fileId=file_id,
                userId=user_id,  # Return the auto-generated user ID
                message="CV uploaded and queued for processing",
                status="queued"
            )
        else:
            logger.error(f"Failed to queue CV: {file_id}")
            return CVUploadResponse(
                fileId=file_id,
                userId=user_id,
                message="CV uploaded but failed to queue for processing",
                status="upload_failed"
            )
        
    except Exception as e:
        logger.error(f"Error uploading CV: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/cv-status/{file_id}")
async def get_cv_status(file_id: str, db: Session = Depends(get_db)):
    """Get CV processing status"""
    candidate = db.query(Candidate).filter(Candidate.CvFileId == file_id).first()
    
    if not candidate:
        return {"status": "not_found", "message": "CV not found"}
    
    evaluation = db.query(CVEvaluation).filter(CVEvaluation.UserProfileId == candidate.UserProfileId).first()
    
    if not evaluation:
        return {"status": "queued", "message": "CV uploaded and queued for processing"}
    
    return {
        "status": "completed",
        "file_id": file_id,
        "user_id": candidate.UserProfileId,
        "model_used": evaluation.ModelUsed,
        "created_at": evaluation.CreatedAt,
        "prompt_category": evaluation.PromptCategory,
        "prompt_version": evaluation.PromptVersion
    }

@router.get("/user-cvs/{user_id}")
async def get_user_cvs(user_id: str, db: Session = Depends(get_db)):
    """Get all CV evaluations for a user"""
    candidates = db.query(Candidate).filter(
        Candidate.UserId == user_id
    ).all()
    
    results = []
    for candidate in candidates:
        evaluation = db.query(CVEvaluation).filter(
            CVEvaluation.UserProfileId == candidate.UserProfileId
        ).first()
        
        if evaluation:
            results.append({
                "file_id": candidate.CvFileId,
                "user_id": candidate.UserProfileId,
                "model_used": evaluation.ModelUsed,
                "created_at": evaluation.CreatedAt,
                "prompt_category": evaluation.PromptCategory,
                "prompt_version": evaluation.PromptVersion,
                "status": "completed"
            })
        else:
            results.append({
                "file_id": candidate.CvFileId,
                "user_id": candidate.UserProfileId,
                "status": "queued"
            })
    
    return {"user_id": user_id, "cvs": results}

@router.get("/user-profile/{user_id}")
async def get_user_profile(user_id: str, db: Session = Depends(get_db)):
    """Get complete user profile with all extracted data"""
    user_profile = db.query(UserProfile).filter(UserProfile.Id == user_id).first()
    
    if not user_profile:
        raise HTTPException(status_code=404, detail="User profile not found")
    
    # Get all related data
    from models.database import (Experience, Education, Skills, ProjectsResearch, 
                                CertificationsLicenses, AwardsAchievements, 
                                VolunteerExtracurricular, Summaries, KeyStrengths)
    
    experiences = db.query(Experience).filter(Experience.UserProfileId == user_id).all()
    educations = db.query(Education).filter(Education.UserProfileId == user_id).all()
    skills = db.query(Skills).filter(Skills.UserProfileId == user_id).all()
    projects = db.query(ProjectsResearch).filter(ProjectsResearch.UserProfileId == user_id).all()
    certifications = db.query(CertificationsLicenses).filter(CertificationsLicenses.UserProfileId == user_id).all()
    awards = db.query(AwardsAchievements).filter(AwardsAchievements.UserProfileId == user_id).all()
    volunteer = db.query(VolunteerExtracurricular).filter(VolunteerExtracurricular.UserProfileId == user_id).all()
    summaries = db.query(Summaries).filter(Summaries.UserProfileId == user_id).all()
    strengths = db.query(KeyStrengths).filter(KeyStrengths.UserProfileId == user_id).all()
    
    return {
        "user_profile": user_profile,
        "experiences": experiences,
        "educations": educations,
        "skills": skills,
        "projects_research": projects,
        "certifications_licenses": certifications,
        "awards_achievements": awards,
        "volunteer_extracurricular": volunteer,
        "summaries": summaries,
        "key_strengths": strengths
    }
