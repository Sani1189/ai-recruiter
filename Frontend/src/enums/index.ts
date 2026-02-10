export enum CandidateNotificationType {
  FEEDBACK_AVAILABLE = "FEEDBACK_AVAILABLE", // Interview score, evaluation, or comments are ready
  NEW_INTERVIEW_PUBLISHED = "NEW_INTERVIEW_PUBLISHED", // A new public interview is available (optional, if you notify users)
  PROFILE_INCOMPLETE = "PROFILE_INCOMPLETE", // Resume or other required fields missing
  SYSTEM_ANNOUNCEMENT = "SYSTEM_ANNOUNCEMENT", // Platform updates, new features, etc.
  ACCOUNT_STATUS_UPDATE = "ACCOUNT_STATUS_UPDATE", // E.g., profile verified, flagged, or suspended
}
