import { InterviewConfigurationWithPrompts } from "@/types/interviewConfiguration";

function replaceTemplateVariables(text: string | undefined, job: any): string {
  if (!text) return "";
  if (!job) return text;
  return text
    .replace(/\{\{jobTitle\}\}/g, job.jobTitle || "Software Engineer")
    .replace(/\{\{companyName\}\}/g, job.companyName || "Osilion AS")
    .replace(/\{\{jobDescription\}\}/g, job.jobDescription || "")
    .replace(/\{\{location\}\}/g, job.location || "")
    .replace(/\{\{experienceLevel\}\}/g, job.experienceLevel || "")
    .replace(/\{\{department\}\}/g, job.department || "")
    .replace(/\{\{employmentType\}\}/g, job.employmentType || "Full-time");
}

export function buildQuestions(
  interviewConfig?: InterviewConfigurationWithPrompts,
  jobPost?: any
): string[] {
  if (!interviewConfig) return [];
  const pick = (text?: string) => (text || "").split(/\n/).map((l) => l.trim()).filter(Boolean);
  const isQ = (l: string) => l.endsWith("?") || /^(\d+\.|[-*•])\s?/.test(l);

  const q = pick(replaceTemplateVariables(interviewConfig.questionsPrompt?.content, jobPost)).filter(isQ);
  const i = pick(replaceTemplateVariables(interviewConfig.instructionPrompt?.content, jobPost)).filter(isQ);
  return [...q, ...i];
}

export function buildDynamicVariables(
  interviewConfig?: InterviewConfigurationWithPrompts,
  jobPost?: any,
  candidateName?: string | null,
  stepLabel?: string
) {
  const questions = buildQuestions(interviewConfig, jobPost);
  const jobTitle = jobPost?.jobTitle || "this position";
  const companyName = jobPost?.companyName || "Osilion AS";
  const stepName = stepLabel || interviewConfig?.name || "Interview";
  
  // Build first message with candidate name greeting
  const greeting = candidateName ? `Hi ${candidateName}, ` : "Hi, ";
  const first_message = jobPost
    ? `${greeting}welcome to your ${jobTitle} interview with ${companyName}. We're about to begin the "${stepName}" segment. Please let me know when you're ready to start.`
    : `${greeting}welcome to your interview. We're about to begin. Please let me know when you're ready to start.`;

  return {
    tone: interviewConfig?.tone || "Professional",
    focus_area: interviewConfig?.focusArea || "General",
    duration: String(interviewConfig?.duration || 30),
    job_title: jobTitle,
    company_name: companyName,
    instruction_prompt: replaceTemplateVariables(interviewConfig?.instructionPrompt?.content, jobPost),
    personality_prompt: replaceTemplateVariables(interviewConfig?.personalityPrompt?.content, jobPost),
    questions_prompt: replaceTemplateVariables(interviewConfig?.questionsPrompt?.content, jobPost),
    questions: questions.join("\n"),
    first_message,
  };
}

export function buildCombinedPromptFromVariables(v: Record<string, string>): string {
  return [
    `Tone: ${v.tone}`,
    `Focus Area: ${v.focus_area}`,
    `Duration: ${v.duration} minutes`,
    "",
    "Interview Instructions:",
    v.instruction_prompt || "",
    "",
    "Interviewer Personality:",
    v.personality_prompt || "",
    "",
    "Question Guidance:",
    v.questions_prompt || "",
  ].join("\n");
}


