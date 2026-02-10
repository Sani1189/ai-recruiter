"use client";

import { Download } from "lucide-react";
import { useEffect, useState } from "react";
import { useParams } from "next/navigation";

import AIFeedback from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/AIFeedback";
import ApplicationDetails from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/ApplicationDetails";
import { CommentForm } from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/CommentForm";
import Header from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/Header";
import JobsTable from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/JobsTable";
import PersonalInfo from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/PersonalInfo";
import Scores from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/Scores";
import Strengths from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/Strengths";
import Weaknesses from "@/components/pages/_recruiter/recruiter/candidates/_candidate_id/Weaknesses";
import BackButton from "@/components/ui/back-button";
import { Button } from "@/components/ui/button";

import { useApi } from "@/hooks/useApi";
import { CandidateView, InterviewView } from "@/types/v2/type.view";
import { downloadFromSasUrl } from "@/lib/fileUtils";

export default function CandidateDetailsPage() {
  const params = useParams();
  const candidateId = params.candidateId as string;
  const api = useApi();

  const [candidate, setCandidate] = useState<CandidateView | null>(null);
  const [scorings, setScorings] = useState<any[]>([]);
  const [rawScorings, setRawScorings] = useState<any[]>([]);
  const [jobApplications, setJobApplications] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [downloadingResume, setDownloadingResume] = useState(false);
  const [derivedFeedback, setDerivedFeedback] = useState<{
    positiveSummary: string;
    negativeSummary: string;
    strengthList: string[];
    weaknessList: string[];
  }>({
    positiveSummary: "",
    negativeSummary: "",
    strengthList: [],
    weaknessList: [],
  });

  useEffect(() => {
    async function fetchData() {
      try {
        setLoading(true);
        
        // Fetch candidate details
        const candidateResponse = await api.get(`/candidate/${candidateId}`);
        const candidateData = candidateResponse.data || candidateResponse;
        
        // Deduplicate by FixedCategory (use max score per category)
        const scorings: any[] = candidateData.scorings || [];
        const groupedMap = new Map<string, number>();
        scorings.forEach((s: any) => {
          const label = (s.fixedCategory || s.category || "Other").toString().trim();
          const val = typeof s.score === "number" ? s.score : null;
          if (val == null) return;
          const current = groupedMap.get(label);
          groupedMap.set(label, current == null ? val : Math.max(current, val));
        });

        const toTitle = (text: string) =>
          text
            .split(" ")
            .map((w) => (w ? w[0].toUpperCase() + w.slice(1) : ""))
            .join(" ");

        const groupedScorings = Array.from(groupedMap.entries()).map(([label, score]) => ({
          fixedCategory: toTitle(label),
          score,
        }));

        // Average based on unique fixed categories (max score per category)
        const numericScores = groupedScorings
          .map((s) => s.score)
          .filter((v) => typeof v === "number") as number[];
        const avgScore =
          numericScores.length > 0
            ? Math.round(
                numericScores.reduce((acc: number, cur: number) => acc + cur, 0) /
                  numericScores.length,
              )
            : 0;

        // Summaries (positive/negative) and key strengths
        const summaries: any[] = candidateData.summaries || [];
        const findByType = (prefix: string) =>
          summaries.find((s) => s.type?.toLowerCase().startsWith(prefix))?.text || "";
        const positiveSummary = findByType("positive");
        const negativeSummary = findByType("negative");
        const weaknessSummary = findByType("weakness");

        const keyStrengths: any[] = candidateData.keyStrengths || [];
        const strengthList = Array.from(
          new Set(
            keyStrengths
              .map(
                (s) =>
                  s.description?.trim() ||
                  s.strengthName?.trim() ||
                  null,
              )
              .filter(Boolean),
          ),
        );

        const weaknessList = weaknessSummary
          ? [weaknessSummary]
          : negativeSummary
            ? [negativeSummary]
            : [];
        setDerivedFeedback({
          positiveSummary,
          negativeSummary,
          strengthList,
          weaknessList,
        });

        const languageSkillScore =
          groupedScorings.find(
            (s) => s.fixedCategory?.toLowerCase() === "language skills",
          )?.score ?? 0;

        // Transform to CandidateView
        const transformedCandidate: CandidateView = {
          id: candidateData.id,
          userProfile: candidateData.userProfile || {
            id: candidateData.userId,
            name: candidateData.userProfile?.name || "Unknown",
            email: candidateData.userProfile?.email || "",
            phoneNumber: candidateData.userProfile?.phoneNumber || null,
            age: candidateData.userProfile?.age || null,
            nationality: candidateData.userProfile?.nationality || null,
            resumeUrl: candidateData.userProfile?.resumeUrl || candidateData.cvFile?.url || null,
          },
          comment: candidateData.comment || null,
          score: {
            avg: avgScore,
            english: languageSkillScore,
            technical: 0,
            communication: 0,
            problemSolving: 0,
          },
        };
        
        setCandidate(transformedCandidate);
        setScorings(groupedScorings);
        setRawScorings(candidateData.scorings || []);
        
        // Fetch job applications for the candidate
        const applicationsResponse = await api.get(`/jobapplication/candidate/${candidateId}`);
        const applications = applicationsResponse.data || applicationsResponse;
        setJobApplications(applications || []);
        
      } catch (err) {
        console.error("Error fetching candidate data:", err);
        setError("Failed to load candidate data");
      } finally {
        setLoading(false);
      }
    }

    if (candidateId) {
      fetchData();
    }
  }, [candidateId]);

  const handleDownloadResume = async () => {
     if (!candidateId || downloadingResume || !candidate) return;
 
     const userProfile = candidate.userProfile || {};
     const name = userProfile.name || "Candidate";
 
     setDownloadingResume(true);
     try {
       // Get SAS URL from backend
       const response = await api.get(`/userprofile/candidate/${candidateId}/resume`);
       
       // Response format: { downloadUrl: string, expiresInMinutes: number }
       // apiClient returns data directly, not wrapped in .data
       const downloadUrl = (response as any).downloadUrl || (response as any).data?.downloadUrl;
       
       if (!downloadUrl) {
         console.error('Download response:', response);
         throw new Error('Download URL not received from server');
       }
       
       // Download directly from Azure using SAS URL
       await downloadFromSasUrl(downloadUrl, `${name}_Resume.pdf`);
     } catch (error) {
       console.error('Failed to download resume:', error);
     } finally {
       setDownloadingResume(false);
     }
   };
  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <p className="text-muted-foreground">Loading candidate details...</p>
        </div>
      </div>
    );
  }

  if (error || !candidate) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <h1 className="text-muted-foreground text-2xl font-bold">
            {error || "Candidate not found"}
          </h1>
          <BackButton />
        </div>
      </div>
    );
  }

    // Transform job applications for display
  const transformedApplications: any[] = jobApplications.map((app: any) => ({
    id: app.id || "",
    jobPostId: "",
    candidateId: candidate.id,
    score: candidate.score,
    feedback: {
      summary: "",
      detailed: "",
      strengths: [],
      weaknesses: [],
    },
    interviewAudioUrl: "",
    interviewQuestions: [],
    completedAt: app.completedAt || new Date().toISOString(),
    duration: 0,
    jobPost: {
      id: app.jobPost?.id || "",
      jobTitle: app.jobPost?.jobTitle || app.jobPostName,
      name: app.jobPostName,
      version: app.jobPostVersion,
      jobType: app.jobPost?.jobType,
      experienceLevel: app.jobPost?.experienceLevel,
      createdAt: app.jobPost?.createdAt || "",
      candidatesCount: 0,
    },
    candidate: candidate,
    currentStep: {} as any,
  }));

  // Build a synthetic application if none exists so UI sections still render
  if (!transformedApplications.length) {
    transformedApplications.push({
      id: "candidate-summary",
      jobPostId: "",
      candidateId: candidate.id,
      score: candidate.score,
      feedback: {
        summary: derivedFeedback.positiveSummary,
        detailed: derivedFeedback.negativeSummary,
        strengths: derivedFeedback.strengthList,
        weaknesses: derivedFeedback.weaknessList,
      },
      interviewAudioUrl: "",
      interviewQuestions: [],
      completedAt: new Date().toISOString(),
      duration: 0,
      jobPost: {
        id: "",
        jobTitle: "",
        name: "",
        version: "",
        jobType: "",
        experienceLevel: "",
        createdAt: "",
        candidatesCount: 0,
      },
      candidate: candidate,
      currentStep: {} as any,
    });
  } else {
    // Attach AI feedback and strengths/weaknesses to the first application for display blocks
    transformedApplications[0] = {
      ...transformedApplications[0],
      score: candidate.score,
      feedback: {
        summary: derivedFeedback.positiveSummary,
        detailed: derivedFeedback.negativeSummary,
        strengths: derivedFeedback.strengthList,
        weaknesses: derivedFeedback.weaknessList,
      },
    };
  }

  const lastApplication = transformedApplications[0];

  return (
    <div className="container space-y-6 py-8">
      <Header candidate={candidate} />

      <div className="flex flex-col gap-6">
        <Button
          variant="link"
          className="w-fit"
          onClick={handleDownloadResume}
          disabled={downloadingResume || !candidate.userProfile?.resumeUrl}
          isLoading={downloadingResume}
        >
          <Download className="mr-2 h-4 w-4" />
          Download Resume
        </Button>

        <JobsTable attendedInterviews={transformedApplications} />

        <div className="grid grid-cols-3 gap-6">
          <PersonalInfo candidate={candidate} />
          <ApplicationDetails candidate={candidate} application={lastApplication} />
          <Scores candidate={candidate} scorings={scorings} rawScorings={rawScorings} />
        </div>

        <div className="grid grid-cols-2 gap-6">
          <Strengths interview={lastApplication} />
          <Weaknesses interview={lastApplication} />
        </div>

        <AIFeedback interview={lastApplication} />

        <CommentForm
          candidateId={candidateId}
          initialComment={candidate.comment?.content ?? ""}
          onSaved={(saved) =>
            setCandidate((prev) => (prev ? { ...prev, comment: saved } : prev))
          }
        />
      </div>
    </div>
  );
}
