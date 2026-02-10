from database import SessionLocal

from models.database import Score
from models.pydantic_models import InterviewScore

class ScoreRepository:
    def _calculate_average(self, score: InterviewScore) -> float:
        # Convert Pydantic model to dict
        data = score.model_dump()

        # Only take numeric score fields dynamically
        score_values = [
            value for _, value in data.items()
            if isinstance(value, (int, float))
        ]

        if not score_values:
            return 0

        return round(sum(score_values) / len(score_values), 2)

    def create_or_update_score(self, score: InterviewScore, interview_id: str) -> Score:
        """Create new score or update existing one (upsert logic)"""
        with SessionLocal() as db:
            # Check if score already exists for this interview
            existing_score = db.execute(
                select(Score).where(Score.InterviewId == interview_id)
            ).scalar_one_or_none()
            if existing_score:
                # Update existing score
                existing_score.English = score.English
                existing_score.Technical = score.Technical
                existing_score.Communication = score.Communication
                existing_score.ProblemSolving = score.ProblemSolving
                existing_score.Average = self._calculate_average(score)
                db.commit()
                db.refresh(existing_score)
                return existing_score
            else:
                # Create new score
                db_score = Score(
                    **score.model_dump(),
                    Average=self._calculate_average(score),
                    InterviewId=interview_id,
                )
                db.add(db_score)
                db.commit()
                db.refresh(db_score)
                return db_score

score_repository = ScoreRepository()