import { InterviewConfigurationWithPrompts } from "@/types/interviewConfiguration";

// Extract questions from interview configuration prompts
export const extractQuestionsFromPrompts = (
  interviewConfig: InterviewConfigurationWithPrompts,
  jobPost: any
): string[] => {
  if (!interviewConfig) return [];

  const questions: string[] = [];

  // Extract from questions prompt
  if (interviewConfig.questionsPrompt?.content) {
    const questionsContent = replaceTemplateVariables(
      interviewConfig.questionsPrompt.content,
      jobPost
    );
    
    // Split by common question patterns
    const questionLines = questionsContent
      .split(/\n/)
      .map(line => line.trim())
      .filter(line => line.length > 0);

    questionLines.forEach(line => {
      // Check if line looks like a question
      if (isQuestionLine(line)) {
        questions.push(line);
      }
    });
  }

  // Extract from instruction prompt if it contains questions
  if (interviewConfig.instructionPrompt?.content) {
    const instructionContent = replaceTemplateVariables(
      interviewConfig.instructionPrompt.content,
      jobPost
    );
    
    const instructionLines = instructionContent
      .split(/\n/)
      .map(line => line.trim())
      .filter(line => line.length > 0);

    instructionLines.forEach(line => {
      if (isQuestionLine(line)) {
        questions.push(line);
      }
    });
  }

  return questions;
};

// Check if a line looks like a question
const isQuestionLine = (line: string): boolean => {
  const trimmed = line.trim();
  
  // Skip empty lines
  if (!trimmed) return false;
  
  // Skip lines that are clearly not questions
  if (trimmed.startsWith('Focus on:') || 
      trimmed.startsWith('Ask questions about:') ||
      trimmed.startsWith('Tailor questions') ||
      trimmed.startsWith('Based on') ||
      trimmed.length < 10) {
    return false;
  }
  
  // Check for question patterns
  return (
    trimmed.endsWith('?') ||
    /^\d+\./.test(trimmed) || // Numbered list
    /^[-*•]/.test(trimmed) || // Bullet points
    trimmed.toLowerCase().includes('what') ||
    trimmed.toLowerCase().includes('how') ||
    trimmed.toLowerCase().includes('why') ||
    trimmed.toLowerCase().includes('describe') ||
    trimmed.toLowerCase().includes('explain') ||
    trimmed.toLowerCase().includes('tell me')
  );
};

// Template replacement function
const replaceTemplateVariables = (text: string, jobPost: any): string => {
  if (!text || !jobPost) return text;
  
  return text
    .replace(/\{\{jobTitle\}\}/g, jobPost.jobTitle || 'Software Engineer')
    .replace(/\{\{companyName\}\}/g, jobPost.companyName || 'Our Company')
    .replace(/\{\{jobDescription\}\}/g, jobPost.jobDescription || '')
    .replace(/\{\{location\}\}/g, jobPost.location || '')
    .replace(/\{\{experienceLevel\}\}/g, jobPost.experienceLevel || '')
    .replace(/\{\{department\}\}/g, jobPost.department || '')
    .replace(/\{\{employmentType\}\}/g, jobPost.employmentType || 'Full-time');
};
