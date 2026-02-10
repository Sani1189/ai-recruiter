from openai import OpenAI
from config import settings
import logging
import json
import PyPDF2
import docx
import io
from typing import Union
from datetime import datetime
import re

from services.cv_prompt_message_builder import build_cv_extraction_messages

def _normalize_date(date_str: str) -> str | None:
    """Normalize various date formats to ISO YYYY-MM-DD or return None for invalid/ongoing dates."""
    if not date_str:
        return None
    date_str = date_str.strip()
    if date_str.lower() in ["present", "current"]:
        return None

    # Match formats like "January 2023", "Feb 2020", "2021", "2021-05"
    month_year_match = re.match(r'^(January|February|March|April|May|June|July|August|September|October|November|December)\s+(\d{4})$', date_str, re.IGNORECASE)
    year_month_match = re.match(r'^(\d{4})-(\d{1,2})$', date_str)
    year_match = re.match(r'^(\d{4})$', date_str)

    if month_year_match:
        month_name, year = month_year_match.groups()
        month_number = datetime.strptime(month_name[:3], "%b").month
        return f"{year}-{month_number:02d}-01"
    elif year_month_match:
        year, month = year_month_match.groups()
        return f"{year}-{int(month):02d}-01"
    elif year_match:
        year = year_match.group(1)
        return f"{year}-01-01"

    # Try parsing exact dates like "2023-01-15"
    try:
        dt = datetime.strptime(date_str, "%Y-%m-%d")
        return dt.date().isoformat()
    except Exception:
        return None


logger = logging.getLogger(__name__)

class OpenAIService:
    def __init__(self):
        self.client = OpenAI(api_key=settings.openai_api_key)
        self.model = settings.openai_model

    def _extract_text_from_file(self, file_content: bytes, file_extension: str) -> str:
        """Extract text content from various file formats."""
        try:
            if file_extension.lower() == 'pdf':
                return self._extract_text_from_pdf(file_content)
            elif file_extension.lower() in ['doc', 'docx']:
                return self._extract_text_from_docx(file_content)
            elif file_extension.lower() == 'txt':
                return file_content.decode('utf-8')
            else:
                try:
                    return file_content.decode('utf-8')
                except UnicodeDecodeError:
                    return file_content.decode('utf-8', errors='ignore')
        except Exception as e:
            logger.error(f"Error extracting text from {file_extension} file: {e}")
            raise Exception(f"Unable to extract text from {file_extension} file: {e}")

    def _extract_text_from_pdf(self, file_content: bytes) -> str:
        """Extract text from PDF file."""
        try:
            pdf_file = io.BytesIO(file_content)
            pdf_reader = PyPDF2.PdfReader(pdf_file)
            text = ""
            for page in pdf_reader.pages:
                text += page.extract_text() + "\n"
            return text.strip()
        except Exception as e:
            logger.error(f"Error extracting text from PDF: {e}")
            raise

    def _extract_text_from_docx(self, file_content: bytes) -> str:
        """Extract text from DOCX file."""
        try:
            doc_file = io.BytesIO(file_content)
            doc = docx.Document(doc_file)
            text = ""
            for paragraph in doc.paragraphs:
                text += paragraph.text + "\n"
            return text.strip()
        except Exception as e:
            logger.error(f"Error extracting text from DOCX: {e}")
            raise

    async def extract_cv_data(
        self,
        file_content: bytes,
        system_prompt_text: str | None,
        scoring_prompt_text: str | None,
        file_extension: str,
        request_id: str = None,
    ) -> dict:
        """Extract CV data using OpenAI with text content."""
        try:
            if request_id is None:
                request_id = "unknown"

            text_content = self._extract_text_from_file(file_content, file_extension)

            if not text_content.strip():
                raise Exception("No text content could be extracted from the file")

            logger.info(f"[{request_id}] Extracted {len(text_content)} characters from {file_extension} file")

            messages = build_cv_extraction_messages(
                system_prompt_text=system_prompt_text,
                scoring_prompt_text=scoring_prompt_text,
                resume_text=text_content,
                request_id=request_id,
            )

            response = self.client.chat.completions.create(
                model=self.model,
                messages=messages,
                response_format={"type": "json_object"},
                temperature=0.1
            )

            response_text = response.choices[0].message.content
            logger.info(f"[{request_id}] Successfully received response from OpenAI.")

            # Parse the response
            extracted_data = json.loads(response_text)

            # Normalize + enforce strict enums
            extracted_data = self._normalize_response_data(extracted_data, request_id)
            logger.info(f"[{request_id}] CV data extracted successfully")

            return extracted_data

        except json.JSONDecodeError as e:
            logger.error(f"[{request_id}] Error parsing OpenAI JSON response: {e}")
            raise Exception(f"Invalid JSON response from OpenAI: {e}")
        except Exception as e:
            logger.error(f"[{request_id}] Error extracting CV data: {e}")
            raise

    def _normalize_response_data(self, data: dict, request_id: str = None) -> dict:
        """Normalize response data and enforce strict enum values."""
        try:
            if request_id is None:
                request_id = "unknown"

            # 1. Handle UserProfile string to array conversions
            if "UserProfile" in data and data["UserProfile"]:
                user_profile = data["UserProfile"]
                for field in ["JobTypePreferences", "RemotePreferences", "Roles"]:
                    if field in user_profile and user_profile[field] is not None:
                        if isinstance(user_profile[field], str):
                            user_profile[field] = [user_profile[field].strip()]
                        elif isinstance(user_profile[field], list):
                            user_profile[field] = [item.strip() for item in user_profile[field] if item]

            # 2. Ensure all array fields exist and are lists
            array_fields = [
                "Experience", "Education", "Skills", "ProjectsResearch",
                "CertificationsLicenses", "AwardsAchievements", "VolunteerExtracurricular",
                "Scoring", "Summaries", "KeyStrengths"
            ]
            for field in array_fields:
                if field not in data or data[field] is None:
                    data[field] = []

            # 3. ENFORCE STRICT ENUM MAPPING
            LEVEL_MAP = {
                "Fluent": "Senior", "High": "Senior", "Proficient": "Senior",
                "Advanced": "Senior", "Intermediate": "Mid", "Beginner": "Junior",
                "Basic": "Junior", "Expert": "Expert", "Senior": "Senior",
                "Mid": "Mid", "Junior": "Junior"
            }
            TYPE_MAP = {
                "Positive": "Positives", "Negative": "Negatives",
                "Positives": "Positives", "Negatives": "Negatives", "Overall": "Overall",
                "Weakness": "Weaknesses", "Weaknesses": "Weaknesses"
            }

            for exp in data.get("Experience", []):
                exp["StartDate"] = _normalize_date(exp.get("StartDate"))
                exp["EndDate"] = _normalize_date(exp.get("EndDate"))

            for edu in data.get("Education", []):
                edu["StartDate"] = _normalize_date(edu.get("StartDate"))
                edu["EndDate"] = _normalize_date(edu.get("EndDate"))

            for cert in data.get("CertificationsLicenses", []):
                cert["DateIssued"] = _normalize_date(cert.get("DateIssued"))
                cert["ValidUntil"] = _normalize_date(cert.get("ValidUntil"))
            
            for award in data.get("AwardsAchievements", []):
                year = award.get("Year")
                if isinstance(year, str) and year.isdigit():
                    award["Year"] = int(year)
                elif isinstance(year, int):
                    award["Year"] = year
                else:
                    award["Year"] = None
            for vol in data.get("VolunteerExtracurricular", []):
                vol["StartDate"] = _normalize_date(vol.get("StartDate"))
                vol["EndDate"] = _normalize_date(vol.get("EndDate"))
            
            for skill in data.get("Skills", []):
                years_exp = skill.get("YearsExperience")
                if isinstance(years_exp, str):
                    try:
                        # Pydantic expects int; truncate floats safely (e.g. "3.5" -> 3)
                        skill["YearsExperience"] = int(float(years_exp))
                    except ValueError:
                        skill["YearsExperience"] = None
                elif isinstance(years_exp, (int, float)):
                    skill["YearsExperience"] = int(years_exp)
                else:
                    skill["YearsExperience"] = None

            # Normalize Scoring numeric fields + handle common key variants
            if isinstance(data.get("Scoring"), list):
                for item in data["Scoring"]:
                    if not isinstance(item, dict):
                        continue

                    # Some models may emit YearsExperience instead of Years for scoring items
                    if item.get("Years") is None and item.get("YearsExperience") is not None:
                        item["Years"] = item.get("YearsExperience")

                    for key in ("Score", "Years"):
                        raw_val = item.get(key)
                        if raw_val is None:
                            continue
                        try:
                            if isinstance(raw_val, str):
                                raw_val = raw_val.strip()
                                if raw_val == "":
                                    item[key] = None
                                    continue
                            item[key] = int(float(raw_val))
                        except Exception:
                            item[key] = None

                    # Score must be 1-10, otherwise null
                    score = item.get("Score")
                    if isinstance(score, int) and (score < 1 or score > 10):
                        item["Score"] = None

            # Fix Scoring.Level
            if isinstance(data.get("Scoring"), list):
                for item in data["Scoring"]:
                    if isinstance(item, dict) and "Level" in item:
                        raw = str(item["Level"]).strip()
                        item["Level"] = LEVEL_MAP.get(raw, "Junior")

            # Fix Summaries.Type
            if isinstance(data.get("Summaries"), list):
                for item in data["Summaries"]:
                    if isinstance(item, dict) and "Type" in item:
                        raw = str(item["Type"]).strip()
                        item["Type"] = TYPE_MAP.get(raw, "Overall")

            logger.info(f"[{request_id}] Response data normalized successfully")
            return data

        except Exception as e:
            logger.error(f"[{request_id}] Error normalizing response data: {e}")
            return data  # Return original on failure (better than crashing)

def get_openai_service() -> OpenAIService:
    """Get OpenAI service instance"""
    return OpenAIService()
