from __future__ import annotations

import logging
from dataclasses import dataclass

logger = logging.getLogger(__name__)


@dataclass(frozen=True)
class CvPromptTemplates:
    """
    Central place for CV extraction prompt defaults + parsing rules.
    Keep this file small and CV-specific (no generic prompt engine).
    """

    default_system_message: str
    default_user_template: str
    default_scoring_instructions: str


DEFAULT_TEMPLATES = CvPromptTemplates(
    default_system_message=(
        "You are a helpful assistant specialized in extracting structured data from resumes and CVs.\n"
        "Your response must be in JSON format only. Analyze the provided CV/resume text and extract all available information.\n"
        "If any information is missing or not available, use null values.\n\n"
        "IMPORTANT: For array fields like JobTypePreferences and RemotePreferences, ALWAYS return arrays even if there's only one item.\n"
        "For example: [\"Full-stack Software Engineer\"] not \"Full-stack Software Engineer\"\n"
        "For example: [\"Remote\"] not \"Remote\"\n"
    ),
    default_scoring_instructions=(
        "- For 'Level' in Scoring: Use ONLY: Junior | Mid | Senior | Expert\n"
        "- For 'Score' and 'Years' fields in Scoring, ensure values are integers only\n"
        "- For JobTypePreferences and RemotePreferences, ALWAYS return arrays: [\"value1\", \"value2\"] not \"value\"\n"
        "- If only one preference exists, still use array format: [\"Single Value\"]\n"
        "- Must give 'Scoring'. 'Scoring' must be an array of objects (even if empty)., MUST not be a single object or null.\n"
        "- Must give a short summary (max 10 sentences) of the positives, a short summary (max 10 sentences) of the negatives, and a short summary (max 10 sentences) of the weaknesses. "
        "For 'Type' in Summaries: Use ONLY: Positives | Negatives | Overall | Weaknesses\n"
        "- Must give a short list of Key strengths (1 sentences per strength) of the candidate.\n"
    ),
    default_user_template=(
        "Please analyze the following CV/resume text and extract all available information.\n"
        "Return the information in the exact JSON format provided below.\n"
        "If any information is missing or not available, use null values.\n\n"
        "CRITICAL OUTPUT RULES (MUST FOLLOW EXACTLY):\n"
        "{scoring_text}\n\n"
        "CV/Resume Text:\n"
        "{resume_text}\n\n"
        "Required JSON format (use PascalCase for all field names):\n"
        "{\n"
        "\"UserProfile\": {\n"
        "\"ResumeUrl\": null,\n"
        "\"Name\": null,\n"
        "\"Email\": null,\n"
        "\"PhoneNumber\": null,\n"
        "\"Age\": null,\n"
        "\"Nationality\": null,\n"
        "\"ProfilePictureUrl\": null,\n"
        "\"Bio\": null,\n"
        "\"JobTypePreferences\": [],\n"
        "\"OpenToRelocation\": null,\n"
        "\"RemotePreferences\": [],\n"
        "\"Roles\": []\n"
        "},\n"
        "\"Candidate\": {\n"
        "\"CvFileId\": null,\n"
        "\"UserProfileId\": null\n"
        "},\n"
        "\"Experience\": [{\"Title\": null,\"Organization\": null,\"Industry\": null,\"Location\": null,\"StartDate\": null,\"EndDate\": null,\"Description\": null}],\n"
        "\"Education\": [{\"Degree\": null,\"Institution\": null,\"FieldOfStudy\": null,\"Location\": null,\"StartDate\": null,\"EndDate\": null}],\n"
        "\"Skills\": [{\"Category\": null,\"SkillName\": null,\"Proficiency\": null,\"YearsExperience\": null,\"Unit\": null}],\n"
        "\"ProjectsResearch\": [{\"Title\": null,\"Description\": null,\"Role\": null,\"TechnologiesUsed\": null,\"Link\": null}],\n"
        "\"CertificationsLicenses\": [{\"Name\": null,\"Issuer\": null,\"DateIssued\": null,\"ValidUntil\": null}],\n"
        "\"AwardsAchievements\": [{\"Title\": null,\"Issuer\": null,\"Year\": null,\"Description\": null}],\n"
        "\"VolunteerExtracurricular\": [{\"Role\": null,\"Organization\": null,\"StartDate\": null,\"EndDate\": null,\"Description\": null}],\n"
        "\"Scoring\": [{\"Category\": null, \"FixedCategory\": null,\"Score\": null,\"Years\": null,\"Level\": null}],\n"
        "\"Summaries\": [{\"Type\": null,\"Text\": null}],\n"
        "\"KeyStrengths\": [{\"StrengthName\": null,\"Description\": null}]\n"
        "}\n"
    ),
)


def build_cv_extraction_messages(
    *,
    system_prompt_text: str | None,
    scoring_prompt_text: str | None,
    resume_text: str,
    request_id: str,
    templates: CvPromptTemplates = DEFAULT_TEMPLATES,
) -> list[dict]:
    """
    Build OpenAI chat messages for CV extraction using 2 DB prompts:
    - system_prompt_text: fixed name in DB; contains system + optional user template via markers.
    - scoring_prompt_text: selected by C#; injected into the user template.

    Supported DB format for system_prompt_text:
    ---SYSTEM---
    (system message)
    ---USER---
    (user template; may include {resume_text} and {scoring_text})

    If markers are not present, we treat the full text as the system message and use default user template.
    """

    scoring_text = (scoring_prompt_text or "").strip() or templates.default_scoring_instructions
    raw = (system_prompt_text or "").strip()

    system_content, user_template = _split_system_prompt(raw)

    if not system_content:
        logger.warning(f"[{request_id}] System prompt missing/empty; using built-in default system message.")
        system_content = templates.default_system_message

    if not user_template:
        user_template = templates.default_user_template

    user_content = _apply_placeholders(user_template, scoring_text=scoring_text, resume_text=resume_text)

    return [{"role": "system", "content": system_content}, {"role": "user", "content": user_content}]


def _split_system_prompt(system_prompt_text: str) -> tuple[str | None, str | None]:
    raw = (system_prompt_text or "").strip()
    if not raw:
        return None, None
    if "---SYSTEM---" in raw and "---USER---" in raw:
        try:
            _, rest = raw.split("---SYSTEM---", 1)
            sys_part, usr_part = rest.split("---USER---", 1)
            return sys_part.strip() or None, usr_part.strip() or None
        except Exception:
            # If parsing fails, fall back to "system only"
            return raw, None
    return raw, None


def _apply_placeholders(template: str, *, scoring_text: str, resume_text: str) -> str:
    # Use simple replace (not .format) so JSON braces in the schema don't break.
    return template.replace("{scoring_text}", scoring_text).replace("{resume_text}", resume_text)


