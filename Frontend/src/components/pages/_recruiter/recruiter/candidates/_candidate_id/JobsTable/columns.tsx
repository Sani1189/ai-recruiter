"use client";

import { ColumnDef } from "@tanstack/react-table";
import { FileText, Headphones, SquareArrowOutUpRight, Bot, User, ChevronLeft, ChevronRight } from "lucide-react";
import Link from "next/link";
import { useState, useRef, useEffect } from "react";

import { buttonVariants } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import InterviewAudioPlayer from "@/components/InterviewAudioPlayer";
import { interviewAudioService } from "@/lib/services/interviewAudioService";
import { useApi } from "@/hooks/useApi";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";

function AudioButton({ 
  candidateId, 
  candidateName, 
  jobName, 
  jobVersion 
}: {
  candidateId: string;
  candidateName: string;
  jobName?: string;
  jobVersion?: number;
}) {
  const [audio, setAudio] = useState(null);
  const [loading, setLoading] = useState(false);
  const api = useApi();

  const load = async () => {
    if (audio || !jobName || !jobVersion) return;
    
    setLoading(true);
    try {
      const data = await interviewAudioService.getJobApplicationWithInterviews(
        api, jobName, jobVersion, candidateId
      );
      setAudio(data);
    } catch (error) {
      console.error('Failed:', error);
    } finally {
      setLoading(false);
    }
  };

  const playlist = audio ? interviewAudioService.createPlaylist(audio, candidateName) : [];

  return (
    <Dialog>
      <DialogTrigger asChild>
        <button
          onClick={load}
          disabled={loading || !jobName || !jobVersion}
          className={buttonVariants({ variant: "ghost", size: "icon" })}
        >
          <Headphones className="h-4 w-4" />
        </button>
      </DialogTrigger>
      
      <DialogContent className="sm:max-w-4xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Interview Audio</DialogTitle>
        </DialogHeader>

        {loading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
            <p className="mt-2 text-muted-foreground">Loading...</p>
          </div>
        ) : (
          <InterviewAudioPlayer playlist={playlist} candidateName={candidateName} />
        )}
      </DialogContent>
    </Dialog>
  );
}

type StepTranscript = {
  stepId: string;
  stepName: string;
  stepNumber: number;
  interviews: Array<{
    interviewId: string;
    transcriptUrl: string;
    transcript: any[];
    loading: boolean;
    error: string | null;
  }>;
};

function TranscriptButton({
  candidateId,
  jobName,
  jobVersion
}: {
  candidateId: string;
  jobName?: string;
  jobVersion?: number;
}) {
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [stepsData, setStepsData] = useState<StepTranscript[]>([]);
  const [error, setError] = useState<string | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const api = useApi();

  const loadAllTranscripts = async () => {
    if (!jobName || !jobVersion) return;

    setLoading(true);
    setError(null);
    try {
      const appData = await interviewAudioService.getJobApplicationWithInterviews(
        api, jobName, jobVersion, candidateId
      );

      if (!appData?.steps) {
        setError("No interview steps found");
        return;
      }

      // Build steps with interviews that have transcriptUrl
      const stepsWithInterviews: StepTranscript[] = appData.steps
        .filter((step: any) => step.interviews && step.interviews.length > 0)
        .map((step: any) => ({
          stepId: step.step?.id || `step-${step.step?.stepNumber}`,
          stepName: step.step?.jobPostStepName || `Step ${step.step?.stepNumber || ""}`,
          stepNumber: step.step?.stepNumber || 0,
          interviews: step.interviews
            .filter((inv: any) => inv.transcriptUrl)
            .map((inv: any) => ({
              interviewId: inv.id,
              transcriptUrl: inv.transcriptUrl,
              transcript: [],
              loading: false,
              error: null,
            })),
        }))
        .filter((step: StepTranscript) => step.interviews.length > 0)
        .sort((a: StepTranscript, b: StepTranscript) => a.stepNumber - b.stepNumber);

      if (stepsWithInterviews.length === 0) {
        setError("No transcripts available for any interview");
        return;
      }

      setStepsData(stepsWithInterviews);

      // Load transcripts for all interviews
      for (const step of stepsWithInterviews) {
        for (const interview of step.interviews) {
          await loadTranscriptForInterview(step.stepId, interview.interviewId, interview.transcriptUrl);
        }
      }
    } catch (err) {
      console.error("Failed to load transcripts:", err);
      setError("Failed to load transcripts");
    } finally {
      setLoading(false);
    }
  };

  const loadTranscriptForInterview = async (
    stepId: string,
    interviewId: string,
    transcriptUrl: string
  ) => {
    // Update loading state for this specific interview
    setStepsData((prev) =>
      prev.map((step) =>
        step.stepId === stepId
          ? {
              ...step,
              interviews: step.interviews.map((inv) =>
                inv.interviewId === interviewId
                  ? { ...inv, loading: true, error: null }
                  : inv
              ),
            }
          : step
      )
    );

    try {
      const response = await api.get(transcriptUrl);
      const data = (response as any)?.data || response;

      const transcriptData = data?.transcript || [];

      if (Array.isArray(transcriptData) && transcriptData.length > 0) {
        setStepsData((prev) =>
          prev.map((step) =>
            step.stepId === stepId
              ? {
                  ...step,
                  interviews: step.interviews.map((inv) =>
                    inv.interviewId === interviewId
                      ? { ...inv, transcript: transcriptData, loading: false, error: null }
                      : inv
                  ),
                }
              : step
          )
        );
      } else {
        setStepsData((prev) =>
          prev.map((step) =>
            step.stepId === stepId
              ? {
                  ...step,
                  interviews: step.interviews.map((inv) =>
                    inv.interviewId === interviewId
                      ? {
                          ...inv,
                          loading: false,
                          error: "Transcript not found in response",
                        }
                      : inv
                  ),
                }
              : step
          )
        );
      }
    } catch (err) {
      console.error(`Failed to load transcript for interview ${interviewId}:`, err);
      setStepsData((prev) =>
        prev.map((step) =>
          step.stepId === stepId
            ? {
                ...step,
                interviews: step.interviews.map((inv) =>
                  inv.interviewId === interviewId
                    ? { ...inv, loading: false, error: "Failed to load transcript" }
                    : inv
                ),
              }
            : step
        )
      );
    }
  };

  const handleOpenChange = (isOpen: boolean) => {
    setOpen(isOpen);
    if (isOpen && stepsData.length === 0 && !loading) {
      loadAllTranscripts();
    }
  };

  const [scrollState, setScrollState] = useState({ left: false, right: false });

  useEffect(() => {
    const el = scrollRef.current;
    if (!el) return;
    const update = () => {
      const { scrollLeft, scrollWidth, clientWidth } = el;
      setScrollState({ left: scrollLeft > 0, right: scrollLeft < scrollWidth - clientWidth - 1 });
    };
    update();
    el.addEventListener("scroll", update);
    const ro = new ResizeObserver(update);
    ro.observe(el);
    return () => { el.removeEventListener("scroll", update); ro.disconnect(); };
  }, [stepsData]);

  const getSpeakerLabel = (entry: any) => {
    return (entry.role|| "").toLowerCase() === 'agent' ? "AI Interviewer" : 'Candidate';
  };

  const isAgent = (entry: any) => {
    return (entry.role|| "").toLowerCase() === "agent";
  };

  const getMessageText = (entry: any) => {
    return entry.text || entry.content || entry.message || entry.transcript || "";
  };

  if (!jobName || !jobVersion) {
    return (
      <button
        className={buttonVariants({ variant: "ghost", size: "icon" })}
        disabled
        title="Transcript unavailable"
      >
        <FileText className="h-4 w-4" />
      </button>
    );
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <button
          className={buttonVariants({ variant: "ghost", size: "icon" })}
          title="View transcript"
        >
          <FileText className="h-4 w-4" />
        </button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>Interview Transcripts</DialogTitle>
        </DialogHeader>

        {loading && stepsData.length === 0 && (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
            <p className="mt-2 text-muted-foreground">Loading transcripts...</p>
          </div>
        )}

        {error && stepsData.length === 0 && (
          <div className="py-4 text-sm text-destructive text-center">{error}</div>
        )}

        {!loading && !error && stepsData.length === 0 && (
          <div className="py-6 text-center text-muted-foreground">No transcripts available.</div>
        )}

        {stepsData.length > 0 && (
          <Tabs defaultValue={stepsData[0]?.stepId} className="flex-1 flex flex-col min-h-0">
            <div className="relative mb-4">
              <div
                ref={scrollRef}
                className="overflow-x-auto scroll-smooth [&::-webkit-scrollbar]:hidden flex justify-center border-1 bg-gray-200/50"
                style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
              >
                <TabsList className="inline-flex  p-2 gap-1 rounded-lg">
                  {stepsData.map((step, index) => (
                    <div key={step.stepId} className="inline-flex items-center gap-1">
                      <TabsTrigger 
                        value={step.stepId}
                        className="shrink-0 px-4 py-2 rounded-lg font-medium data-[state=active]:bg-brand-secondary/10 data-[state=active]:text-brand-secondary flex items-center gap-2"
                      >
                        <span className="w-6 h-6 rounded-full bg-background flex items-center justify-center text-xs font-semibold">
                          {index + 1}
                        </span>
                        <span>{step.stepName}</span>
                        {step.interviews.length > 1 && (
                          <span className="ml-1 text-xs opacity-70">({step.interviews.length})</span>
                        )}
                      </TabsTrigger>
                      {index < stepsData.length - 1 && (
                        <ChevronRight className="h-4 w-4 text-muted-foreground/50 mx-1" />
                      )}
                    </div>
                  ))}
                </TabsList>
              </div>
              
              {scrollState.left && (
                <button
                  onClick={() => scrollRef.current?.scrollBy({ left: -200, behavior: "smooth" })}
                  className="absolute left-0 top-0 h-full w-8 bg-background/80 flex items-center z-10"
                >
                  <ChevronLeft className="h-4 w-4" />
                </button>
              )}
              
              {scrollState.right && (
                <button
                  onClick={() => scrollRef.current?.scrollBy({ left: 200, behavior: "smooth" })}
                  className="absolute right-0 top-0 h-full w-8 bg-background/80 flex items-center justify-end z-10"
                >
                  <ChevronRight className="h-4 w-4" />
                </button>
              )}
            </div>

            {stepsData.map((step) => (
              <TabsContent
                key={step.stepId}
                value={step.stepId}
                className="flex-1 overflow-y-auto mt-4 space-y-6"
              >
                {step.interviews.map((interview, interviewIdx) => (
                  <div key={interview.interviewId} className="space-y-3">
                    {step.interviews.length > 1 && (
                      <div className="text-sm font-semibold text-muted-foreground pb-2 border-b">
                        Interview {interviewIdx + 1}
                      </div>
                    )}

                    {interview.loading && (
                      <div className="text-center py-8">
                        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary mx-auto"></div>
                        <p className="mt-2 text-xs text-muted-foreground">Loading transcript...</p>
                      </div>
                    )}

                    {interview.error && (
                      <div className="py-4 text-sm text-destructive text-center">
                        {interview.error}
                      </div>
                    )}

                    {!interview.loading && !interview.error && interview.transcript.length === 0 && (
                      <div className="py-6 text-center text-muted-foreground text-sm">
                        No transcript available.
                      </div>
                    )}

                    {!interview.loading && !interview.error && interview.transcript.length > 0 && (
                      <div className="space-y-4 py-2">
                        {interview.transcript.map((entry, idx) => {
                          const speaker = getSpeakerLabel(entry);
                          const text = getMessageText(entry);
                          const agent = isAgent(entry);

                          return (
                            <div
                              key={idx}
                              className={`flex gap-3 ${
                                agent ? "flex-row" : "flex-row-reverse"
                              }`}
                            >
                              {/* Avatar/Icon */}
                              <div
                                className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center ${
                                  agent
                                    ? "bg-primary/10 text-primary"
                                    : "bg-brand-secondary/10 text-brand-secondary"
                                }`}
                              >
                                {agent ? (
                                  <Bot className="h-4 w-4" />
                                ) : (
                                  <User className="h-4 w-4" />
                                )}
                              </div>

                              {/* Message Content */}
                              <div className={`flex-1 min-w-0 ${agent ? "" : "flex justify-end"}`}>
                                <div
                                  className={`inline-block max-w-[85%] rounded-2xl px-4 py-3 shadow-sm ${
                                    agent
                                      ? "bg-primary/5 border border-primary/20 text-foreground rounded-tl-sm"
                                      : "bg-brand-secondary/10 border border-brand-secondary/20 text-foreground rounded-tr-sm"
                                  }`}
                                >
                                  <div className="text-xs font-semibold mb-1.5 opacity-70">
                                    {speaker}
                                  </div>
                                  <p className="text-sm leading-relaxed whitespace-pre-wrap break-words">
                                    {text || "[No content]"}
                                  </p>
                                </div>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                ))}
              </TabsContent>
            ))}
          </Tabs>
        )}
      </DialogContent>
    </Dialog>
  );
}

export const columns: ColumnDef<any>[] = [
  {
    id: "select",
    header: ({ table }) => (
      <Checkbox
        checked={
          table.getIsAllPageRowsSelected() ||
          (table.getIsSomePageRowsSelected() && "indeterminate")
        }
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
        aria-label="Select all"
        className="translate-y-[2px]"
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(value) => row.toggleSelected(!!value)}
        aria-label="Select row"
        className="translate-y-[2px]"
      />
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: "jobPost.jobTitle",
    id: "title",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job Title" />
    ),
    cell: ({ row }) => {
      const interview = row.original;
      const job = interview.jobPost;
      const candidateId = interview.candidateId || row.original.candidate?.id;
      const href = job?.name && job?.version && candidateId
        ? `/recruiter/debrief/${job.name}/${job.version}?candidateId=${candidateId}`
        : job?.name && job?.version
        ? `/recruiter/debrief/${job.name}/${job.version}`
        : '#';
      return (
        <Link
          href={href}
          className="hover:underline"
        >
          {row.getValue("title")}
        </Link>
      );
    },
  },
  {
    accessorKey: "completedAt",
    id: "appliedDate",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Applied Date" />
    ),
    cell: ({ row }) => {
      const date = row.getValue("appliedDate") as string;
      if (!date) return <span className="px-4">-</span>;
      return (
        <span className="px-4">
          {new Date(date).toLocaleDateString()}
        </span>
      );
    },
  },
  {
    accessorKey: "jobPost.version",
    id: "version",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Version" />
    ),
    cell: ({ row }) => {
      const version = row.getValue("version") as number;
      return <span className="px-4">v{version}</span>;
    },
  },
  {
    accessorKey: "jobPost.jobType",
    id: "jobType",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job Type" />
    ),
    cell: ({ row }) => {
      const jobType = row.getValue("jobType") as string;
      return <span className="px-4">{jobType || "N/A"}</span>;
    },
  },
  {
    accessorKey: "jobPost.experienceLevel",
    id: "experienceLevel",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Experience Level" />
    ),
    cell: ({ row }) => {
      const experienceLevel = row.getValue("experienceLevel") as string;
      return <span className="px-4">{experienceLevel || "N/A"}</span>;
    },
  },
  {
    accessorKey: "score.avg",
    id: "averageScore",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Average Score" />
    ),
    cell: ({ row }) => {
      const avgScore = row.original.score?.avg || 0;
      return <span className="px-4">{avgScore.toFixed(1)}</span>;
    },
  },
  {
    accessorKey: "actions",
    header: () => <span className="px-3">Actions</span>,
    cell: ({ row }) => {
      const item = row.original;
      const job = item.jobPost;

      return (
        <div className="flex items-center justify-start gap-2">
          <TranscriptButton
            candidateId={item.candidateId || ""}
            jobName={job?.name}
            jobVersion={job?.version}
          />

          <AudioButton
            candidateId={item.candidateId || ""}
            candidateName={item.candidate?.userProfile?.name || "Candidate"}
            jobName={job?.name}
            jobVersion={job?.version}
          />

          <Link 
            href={job?.name && job?.version ? `/recruiter/jobs/${job.name}/${job.version}` : `#`}
            className={buttonVariants({ variant: "ghost", size: "icon" })}
          >
            <SquareArrowOutUpRight className="h-4 w-4" />
          </Link>
        </div>
      );
    },
  },
];
