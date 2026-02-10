import asyncio
import json
import logging
from typing import Any

from openai import AsyncOpenAI
from typing import List

from database import SessionLocal
from services.prompt_resolver import PromptRef, prompt_resolver
from config import settings
from models.pydantic_models import ScoringCriteria, TranscriptItem, InterviewScore
from repositories.score_repository import score_repository
from repositories.interview_repository import interview_repository

logger = logging.getLogger(__name__)

class InterviewScoringService:
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.openai_api_key)
        self.model = settings.openai_model
        self.max_output_tokens = settings.max_output_tokens
        self.text_verbosity = settings.text_verbosity
    
    def _build_scoring_prompt(self, transcript: List[TranscriptItem], criteria: ScoringCriteria) -> str:

        conversation_lines = []
        for item in transcript:
            role_label = "**Interviewer**" if item.role == "agent" else "**Candidate**"
            conversation_lines.append(f"{role_label}: {item.message} [time_in_call_secs={item.time_in_call_secs}]")

        transcript_conversation = chr(10).join(conversation_lines)
        technical_weight = criteria.Technical
        communication_weight = criteria.Communication
        problem_solving_weight = criteria.ProblemSolving
        english_weight = criteria.English

        db = SessionLocal()
        

        scoring_prompt = prompt_resolver.resolve(
                db,
                PromptRef(name="Transcript Scoring Prompt"),
                request_id="request_id",
                default_content="",
                allow_latest=True,
            )
        
        if not scoring_prompt:
            raise ValueError("Scoring prompt not found.")
        
        prompt_content = scoring_prompt.content

        # replace variables in the prompt content
        replacements = {
            "{transcript_conversation}": transcript_conversation,
            "{technical_weight}": str(technical_weight),
            "{communication_weight}": str(communication_weight),
            "{problem_solving_weight}": str(problem_solving_weight),
            "{english_weight}": str(english_weight)
        }
        for key, value in replacements.items():
            prompt_content = prompt_content.replace(key, value)
        
        return prompt_content
    
    async def _score_transcript(
        self, 
        transcript: List[TranscriptItem], 
        criteria: ScoringCriteria | None = None
    ) -> InterviewScore:
        if criteria is None:
            criteria = ScoringCriteria()


        DEBUG = False  # Set to True to enable debug mode with fixed responses
        
        prompt = self._build_scoring_prompt(transcript, criteria)

        last_error: Exception | None = None
        max_output_tokens = self.max_output_tokens
        for attempt in range(1, 3):
            try:
                # If in debug mode, return a fixed response
                if DEBUG:
                    return InterviewScore(
                        Communication=25,
                        Technical=35,
                        ProblemSolving=25,
                        English=15
                    )

                # Responses API + Structured Outputs
                parse_kwargs: dict[str, Any] = {
                    "model": self.model,
                    "instructions": (
                        "You are an expert HR and technical interviewer. You evaluate candidates fairly and provide constructive, detailed feedback. "
                        "Return ONLY valid JSON that matches the schema."
                    ),
                    "input": prompt,
                    "max_output_tokens": max_output_tokens,
                    "text": {"verbosity": self.text_verbosity},
                    "text_format": InterviewScore,
                }
                response = await self.client.responses.parse(**parse_kwargs)

                status = getattr(response, "status", None)
                if status == "incomplete":
                    incomplete = getattr(response, "incomplete_details", None)
                    reason = getattr(incomplete, "reason", None)
                    if attempt < 2 and reason == "max_output_tokens" and max_output_tokens < 8000:
                        max_output_tokens = min(8000, max_output_tokens * 2)
                        await asyncio.sleep(0.2)
                        continue
                    raise ValueError(f"Incomplete response from OpenAI: {incomplete}")

                parsed = getattr(response, "output_parsed", None)
                if parsed is None:
                    # If parsing failed for any reason, surface the best available text.
                    output_text = getattr(response, "output_text", None)
                    raise ValueError(f"No parsed output from OpenAI. output_text={output_text!r}")
                
                return parsed

            except Exception as e:
                last_error = e
                if attempt < 2:
                    await asyncio.sleep(0.5)
                    continue
                break

        raise ValueError(f"OpenAI API error: {last_error}")
    
    async def process_transcript(
        self,
        transcript: List[TranscriptItem],
        conv_id: str
    ) -> tuple[bool, str]:
        try:
            interview = interview_repository.get_by_conv_id(conv_id)
            if not interview:
                raise ValueError(f"No Interview found for conv_id={conv_id}")
            
            criteria = ScoringCriteria(
                Technical=35,
                Communication=25,
                ProblemSolving=25,
                English=15
            )
            score = await self._score_transcript(transcript, criteria)

            score_repository.create_or_update_score(score, interview_id=interview.Id)

            return True, "Interview scored successfully"
        except Exception as e:
            error_msg = f"Error in interview scoring service: {e}"
            logging.error(error_msg)
            return False, error_msg

interview_scoring_service = InterviewScoringService()
