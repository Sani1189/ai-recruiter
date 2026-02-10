import {
  Candidate,
  CandidateView,
  Interview,
  InterviewView,
  JobPost,
  Score,
  UserProfile,
} from "@/types/v2/type.view";

/* ========= VIEW LAYER GENERATOR ========= */
function generateViewLayer(
  jobPosts: JobPost[],
  users: UserProfile[],
  candidates: Candidate[],
  interviews: Interview[],
) {
  // Build CandidateView list
  const candidateViews: CandidateView[] = candidates.map((candidate) => {
    const userProfile = users.find((u) => u.id === candidate.userId)!;

    // Find all interviews for this candidate
    const candidateInterviews = interviews.filter(
      (i) => i.candidateId === candidate.id,
    );

    // Calculate average scores across all interviews
    const scoreTotals = {
      english: 0,
      technical: 0,
      communication: 0,
      problemSolving: 0,
    };
    candidateInterviews.forEach((iv) => {
      scoreTotals.english += iv.score.english;
      scoreTotals.technical += iv.score.technical;
      scoreTotals.communication += iv.score.communication;
      scoreTotals.problemSolving += iv.score.problemSolving;
    });

    const count = candidateInterviews.length || 1; // avoid div by 0
    const avgScore: Score = {
      english: Math.round(scoreTotals.english / count),
      technical: Math.round(scoreTotals.technical / count),
      communication: Math.round(scoreTotals.communication / count),
      problemSolving: Math.round(scoreTotals.problemSolving / count),
      avg: Math.round(
        (scoreTotals.english +
          scoreTotals.technical +
          scoreTotals.communication +
          scoreTotals.problemSolving) /
          (4 * count),
      ),
    };

    return {
      id: candidate.id,
      userProfile,
      score: avgScore,
    };
  });

  // Build InterviewView list
  const interviewViews: InterviewView[] = interviews.map((interview) => {
    const jobPost = jobPosts.find((jp) => jp.id === interview.jobPostId)!;
    const candidateView = candidateViews.find(
      (cv) => cv.id === interview.candidateId,
    )!;

    return {
      id: interview.id,
      jobPost,
      candidate: candidateView,
      score: interview.score,
      feedback: interview.feedback,
      interviewAudioUrl: interview.interviewAudioUrl,
      interviewQuestions: interview.interviewQuestions,
      completedAt: interview.completedAt,
      duration: interview.duration,
      currentStep: jobPost.interviewSteps?.[interview.currentStepIndex] ?? "Interview",
      comment: {
        content: "Generated comment",
        entityId: interview.id,
        entityType: 0,
        parentCommentId: null,
        id: `${interview.id}-comment`,
        createdAt: interview.completedAt,
        updatedAt: interview.completedAt,
        createdBy: candidateView.userProfile.id,
        updatedBy: candidateView.userProfile.id,
      },
    };
  });

  return { candidateViews, interviewViews };
}

export { generateViewLayer };
