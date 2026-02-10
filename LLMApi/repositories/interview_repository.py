from sqlalchemy import select

from database import SessionLocal
from config import settings

from models.database import Interview

class InterviewRepository:
    def get_by_conv_id(self, conv_id: str) -> Interview | None:
        with SessionLocal() as db:
            transcript_url = f"{settings.backend_base_url}/api/candidate/ai-interview/conversations/conv_{conv_id}"
            result = db.execute(
                select(Interview).where(Interview.TranscriptUrl == transcript_url)
            )
            return result.scalar_one_or_none()

interview_repository = InterviewRepository()