from services.azure_storage import azure_storage
from services.openai_service import get_openai_service
from database import get_db, SessionLocal, set_current_user
from models.database import (
    CvEvaluation, UserProfile, Candidate, Experience, Education, Skill,
    ProjectsResearch, CertificationsLicenses, AwardsAchievements,
    VolunteerExtracurricular, Scoring, Summary, KeyStrength, Prompt, File
)
from models.pydantic_models import CVExtractionResponse
from sqlalchemy.orm import Session
from sqlalchemy import and_
import json
import logging
from datetime import datetime
from typing import Dict, Any
import uuid
import asyncio
from config import settings
from core.file_helper import FilePathHelper
from services.prompt_resolver import PromptRef, prompt_resolver

logger = logging.getLogger(__name__)

class CVProcessor:
    def __init__(self):
        self._openai_service = None
    
    @property
    def openai_service(self):
        if self._openai_service is None:
            self._openai_service = get_openai_service()
        return self._openai_service
    
    async def process_cv_direct(self, processing_data: Dict[str, Any], db: Session = None, request_id: str | None = None):
        """Process a CV directly without queue"""
        request_id = request_id or str(uuid.uuid4())
        
        if db is None:
            db = SessionLocal()
            session_created = True
        else:
            session_created = False
        
        try:
            user_id = processing_data.get('userId')
            file_id = processing_data.get('fileId')
            file_extension = processing_data.get('fileExtension')
            # "prompt*" coming from C# backend (support both camelCase + snake_case)
            prompt_name = processing_data.get('promptName') or processing_data.get('prompt_name')
            prompt_category = processing_data.get('promptCategory') or processing_data.get('prompt_category')
            prompt_version = processing_data.get('promptVersion') or processing_data.get('prompt_version')
            blob_path = processing_data.get('blobPath')
            file_content = processing_data.get('fileContent')
            
            if not all([user_id, file_id, file_extension, file_content]):
                raise ValueError("Missing required fields: userId, fileId, fileExtension, fileContent")
            
            logger.info(f"[{request_id}] Processing CV for user {user_id}, file {file_id}")
            
            # === 1. Resolve prompts ===
            # System prompt: fixed name, latest version
            system_prompt = prompt_resolver.resolve(
                db,
                PromptRef(name="CVExtractionSystemInstructions"),
                request_id=request_id,
                default_content="",
                allow_latest=True,
            )

            # Scoring prompt: comes from C# (name/category/version) with safe fallback
            scoring_default = ""
            scoring_name = prompt_name or "CVExtractionScoringInstructions"
            scoring_prompt = prompt_resolver.resolve(
                db,
                PromptRef(name=scoring_name, category=prompt_category, version=prompt_version),
                request_id=request_id,
                default_content=scoring_default,
                allow_latest=True,
            )
            
            # === 2. Extract with retry ===
            max_retries = settings.max_retry_attempts
            extraction_result = None
            
            for attempt in range(max_retries):
                try:
                    extraction_result = await self.openai_service.extract_cv_data(
                        file_content,
                        system_prompt.content,
                        scoring_prompt.content,
                        file_extension,
                        request_id=request_id,
                    )
                    break
                except Exception as e:
                    logger.warning(f"[{request_id}] OpenAI attempt {attempt + 1} failed: {e}")
                    if attempt == max_retries - 1:
                        raise
                    await asyncio.sleep(2 ** attempt)
            
            # === 3. Validate with Pydantic ===
            cv_response = CVExtractionResponse.model_validate(extraction_result)
            
            # Set audit context for database operations (use email, matching C# pattern)
            user_profile = db.query(UserProfile).filter(
                UserProfile.Id == user_id,
                UserProfile.IsDeleted == False
            ).first()
            user_email = user_profile.Email if user_profile and user_profile.Email else user_id
            set_current_user(user_email)
            
            # === 4. Save raw JSON to blob ===
            json_folder_path, json_file_name, json_success = azure_storage.upload_json_response(
                extraction_result, user_id, file_id
            )
            
            # === 5. Save File record ===
            existing_file = db.query(File).filter(
                File.Id == file_id,
                File.IsDeleted == False
            ).with_for_update().first()
            
            # Parse blob_path using helper
            folder_path, file_name = FilePathHelper.parse_blob_path(blob_path)
            
            if not existing_file:
                file_record = File(
                    Id=file_id,
                    Container=settings.azure_storage_container_name,
                    FolderPath=folder_path,
                    FilePath=file_name,  # Only filename, not full path
                    Extension=file_extension,
                    MbSize=len(file_content) // (1024 * 1024),
                    StorageAccountName=settings.storage_account_name
                )
                db.add(file_record)
            else:
                # Update existing file with proper path structure (FilePath = filename only)
                existing_file.FolderPath = folder_path
                existing_file.FilePath = file_name  # Only filename
                existing_file.MbSize = len(file_content) // (1024 * 1024)
                existing_file.StorageAccountName = settings.storage_account_name
            
            # === 6. Save CV Evaluation ===
            resolved_prompt_category = prompt_category or scoring_prompt.category or "cv_extraction"
            resolved_prompt_version = (
                int(prompt_version)
                if isinstance(prompt_version, int) and prompt_version >= 1
                else (scoring_prompt.version or 1)
            )
            cv_evaluation = CvEvaluation(
                UserProfileId=user_id,
                PromptCategory=resolved_prompt_category,
                PromptVersion=resolved_prompt_version,
                FileId=file_id,
                ModelUsed=self.openai_service.model,
                ResponseJson=json.dumps(extraction_result or []),  # Store raw response
            )
            db.add(cv_evaluation)
            db.flush()
            
            # === 7. Save Structured Data ===
            await self._save_extracted_data(db, user_id, cv_response, cv_evaluation.Id, request_id)
            
            db.commit()
            logger.info(f"[{request_id}] CV processed successfully for user {user_id}")
            
        except Exception as e:
            db.rollback()
            logger.error(f"[{request_id}] Error processing CV: {e}")
            raise
        finally:
            if session_created:
                db.close()
    
    async def process_cv(self, message: Dict[str, Any]):
        """DEPRECATED: Use process_cv_direct"""
        raise DeprecationWarning("Use process_cv_direct instead")
    
    async def _save_extracted_data(self, db: Session, user_id: str, cv_data: CVExtractionResponse, cv_evaluation_id: str, request_id: str):
        """Save extracted CV data to database tables"""
        
        # === UserProfile ===
        # UserProfile already exists from C# backend, just update it (excluding Email - unique constraint)
        if cv_data.UserProfile:
            user_profile = db.query(UserProfile).filter(
                UserProfile.Id == user_id,
                UserProfile.IsDeleted == False
            ).with_for_update().first()
            
            if user_profile:
                profile_data = cv_data.UserProfile.model_dump(exclude_none=True)
                # Exclude Email - it's unique and managed by C# backend
                profile_data.pop('Email', None)
                # Exclude ResumeUrl - it's managed by the system through CV upload process (set in function_app.py)
                profile_data.pop('ResumeUrl', None)
                
                for field, value in profile_data.items():
                    if hasattr(user_profile, field):
                        if field in ['JobTypePreferences', 'RemotePreferences', 'Roles']:
                            setattr(user_profile, field, value or [])
                        else:
                            setattr(user_profile, field, value)

        # === Candidate (1:1) ===
        if cv_data.Candidate:
            candidate = db.query(Candidate).filter(
                Candidate.UserId == user_id,
                Candidate.IsDeleted == False
            ).with_for_update().first()
            
            if not candidate:
                candidate = Candidate(
                    Id=str(uuid.uuid4()),
                    UserId=user_id,
                    CandidateId=""  # Will be set by C# backend if needed
                )
                db.add(candidate)
            
            candidate_data = cv_data.Candidate.model_dump(exclude_none=True)
            for field, value in candidate_data.items():
                if hasattr(candidate, field):
                    setattr(candidate, field, value)
        
        # === Clear old data (soft-delete style) ===
        db.query(Experience).filter(
            Experience.UserProfileId == user_id,
            Experience.IsDeleted == False
        ).update({Experience.IsDeleted: True})
        
        db.query(Education).filter(Education.UserProfileId == user_id, Education.IsDeleted == False).update({Education.IsDeleted: True})
        db.query(Skill).filter(Skill.UserProfileId == user_id, Skill.IsDeleted == False).update({Skill.IsDeleted: True})
        db.query(ProjectsResearch).filter(ProjectsResearch.UserProfileId == user_id, ProjectsResearch.IsDeleted == False).update({ProjectsResearch.IsDeleted: True})
        db.query(CertificationsLicenses).filter(CertificationsLicenses.UserProfileId == user_id, CertificationsLicenses.IsDeleted == False).update({CertificationsLicenses.IsDeleted: True})
        db.query(AwardsAchievements).filter(AwardsAchievements.UserProfileId == user_id, AwardsAchievements.IsDeleted == False).update({AwardsAchievements.IsDeleted: True})
        db.query(VolunteerExtracurricular).filter(VolunteerExtracurricular.UserProfileId == user_id, VolunteerExtracurricular.IsDeleted == False).update({VolunteerExtracurricular.IsDeleted: True})
        db.query(Scoring).filter(Scoring.CvEvaluationId == cv_evaluation_id).delete()
        db.query(Summary).filter(Summary.UserProfileId == user_id, Summary.IsDeleted == False).update({Summary.IsDeleted: True})
        db.query(KeyStrength).filter(KeyStrength.UserProfileId == user_id, KeyStrength.IsDeleted == False).update({KeyStrength.IsDeleted: True})

        # === Save new data ===
        def safe_save(model_class, items, extra_fields=None):
            if not items:
                return
            for item in items:
                data = item.model_dump(exclude_none=True)
                if not data:
                    continue
                try:
                    kwargs = {"UserProfileId": user_id}
                    if extra_fields:
                        kwargs.update(extra_fields)
                    kwargs.update(data)
                    obj = model_class(**{k: v for k, v in kwargs.items() if hasattr(model_class, k)})
                    db.add(obj)
                except Exception as e:
                    logger.warning(f"[{request_id}] Error saving {model_class.__name__}: {e}")
        
        safe_save(Experience, cv_data.Experience)
        safe_save(Education, cv_data.Education)
        safe_save(Skill, cv_data.Skills)
        safe_save(ProjectsResearch, cv_data.ProjectsResearch)
        safe_save(CertificationsLicenses, cv_data.CertificationsLicenses)
        safe_save(AwardsAchievements, cv_data.AwardsAchievements)
        safe_save(VolunteerExtracurricular, cv_data.VolunteerExtracurricular)
        safe_save(Scoring, cv_data.Scoring, extra_fields={"CvEvaluationId": cv_evaluation_id})
        safe_save(Summary, cv_data.Summaries)
        safe_save(KeyStrength, cv_data.KeyStrengths)


# Singleton
cv_processor = CVProcessor()
