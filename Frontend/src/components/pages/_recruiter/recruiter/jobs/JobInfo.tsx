import { Calendar, Maximize2, Users, Shield, Target, MapPin } from "lucide-react";
import JobStepsKanban from "./JobStepsKanban";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";

import { useCountries } from "@/hooks/useCountries";
import { formatDate } from "@/lib/utils";
import JobStepsPipelineView from "./JobStepsPipelineView";

interface JobPost {
  name: string;
  version: number;
  jobTitle: string;
  jobType: string;
  experienceLevel: string;
  jobDescription: string;
  policeReportRequired?: boolean;
  maxAmountOfCandidatesRestriction?: number;
  minimumRequirements?: string[];
  candidateCount?: number;
  status?: string;
  originCountryCode?: string | null;
  countryExposureCountryCodes?: string[];
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  assignedSteps?: Array<{
    stepNumber: number;
    status: string;
    stepDetails: {
      name: string;
      version: number;
      stepType: string;
      isInterview: boolean;
      participant?: string;
    };
  }>;
}

interface JobStepCandidate {
  applicationId: string;
  candidateId: string;
  candidateSerial: string;
  candidateName: string;
  candidateEmail: string;
  appliedAt: string;
  completedAt?: string;
  status: string;
  currentStep?: number;
}

const VISIBLE_EXPOSURE_COUNT = 3;

export default function JobInfo({
  jobPost,
  candidates = [],
  onRefreshNeeded,
}: {
  jobPost: JobPost;
  candidates?: JobStepCandidate[];
  onRefreshNeeded?: () => void;
}) {
  const { countries } = useCountries();
  const codeToName = (code: string) =>
    countries.find((c) => c.countryCode === code)?.name ?? code;

  const exposureCodes = jobPost.countryExposureCountryCodes ?? [];
  const visibleCodes = exposureCodes.slice(0, VISIBLE_EXPOSURE_COUNT);
  const remainingCount = exposureCodes.length - VISIBLE_EXPOSURE_COUNT;

  // Sample job description for demonstration (replace with actual content)
  const sampleJobDescription = jobPost.jobDescription || "We are seeking a talented and experienced professional to join our dynamic team. This role offers the opportunity to work on cutting-edge projects and collaborate with industry experts. The ideal candidate will bring innovative thinking, strong technical skills, and a passion for delivering exceptional results.";
  
  // Sample minimum requirements for demonstration (replace with actual content)
  const sampleRequirements = jobPost.minimumRequirements?.length 
    ? jobPost.minimumRequirements 
    : [
        "Bachelor's degree in Computer Science or related field",
        "3+ years of professional development experience", 
        "Strong proficiency in modern programming languages",
        "Experience with cloud platforms and DevOps practices",
        "Excellent communication and teamwork skills"
      ];

  const DESC_AND_REQUIREMENTS = [
    {
      title: "Job Description",
      content: sampleJobDescription,
    },
    {
      title: "Minimum Requirements",
      content: sampleRequirements,
      isList: true,
    },
  ];

  return (
    <div className="xs:grid-cols-5 grid gap-4">
      <Card className="xs:col-span-5 gap-3 lg:col-span-2">
        <CardHeader>
          <CardTitle className="text-lg">Job Post Details</CardTitle>
        </CardHeader>

        <CardContent className="xs:grid-cols-2 grid gap-x-5 gap-y-3">
          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Job Type:</span>
              <Badge variant="secondary">{jobPost.jobType}</Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Experience Level:</span>
              <Badge variant="outline">{jobPost.experienceLevel}</Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Max Candidates:</span>
              <Badge variant="secondary" className="flex items-center gap-x-2">
                <Target className="text-muted-foreground h-3 w-3" />
                <span>{jobPost.maxAmountOfCandidatesRestriction || "No limit"}</span>
              </Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Police Report:</span>
              <Badge variant={jobPost.policeReportRequired ? "default" : "outline"} className="flex items-center gap-x-2">
                <Shield className="text-muted-foreground h-3 w-3" />
                <span>{jobPost.policeReportRequired ? "Required" : "Not Required"}</span>
              </Badge>
            </div>

            
            <div className="flex justify-between items-center">
              <span className="text-muted-foreground text-sm">Origin country:</span>
              <Badge variant="outline" className="flex items-center gap-x-2 font-mono text-xs">
                <MapPin className="text-muted-foreground h-3 w-3" />
                {jobPost.originCountryCode ?? "—"}
              </Badge>
            </div>
          </div>

          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Version:</span>
              <Badge variant="outline">v{jobPost.version}</Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Name:</span>
              <Badge variant="outline" className="font-mono text-xs">
                {jobPost.name}
              </Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">Created At:</span>
              <Badge variant="secondary" className="flex items-center gap-x-2">
                <Calendar className="text-muted-foreground h-4 w-4" />
                <span>{formatDate(jobPost.createdAt)}</span>
              </Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground text-sm">
                Total Applicants:
              </span>
              <Badge variant="secondary" className="flex items-center gap-x-2">
                <Users className="text-muted-foreground h-4 w-4" />
                <span>{jobPost.candidateCount || 0}</span>
              </Badge>
            </div>

            <div className="flex justify-between items-start gap-2">
              <span className="text-muted-foreground text-sm shrink-0">Exposure countries:</span>
              <div className="flex flex-wrap gap-1 justify-end items-center">
                {exposureCodes.length === 0 ? (
                  <span className="text-muted-foreground text-xs">—</span>
                ) : (
                  <>
                    {visibleCodes.map((code) => (
                      <Badge key={code} variant="secondary" className="font-mono text-xs">
                        {code.toUpperCase()}
                      </Badge>
                    ))}
                    {remainingCount > 0 && (
                      <Dialog>
                        <DialogTrigger asChild>
                          <Button
                            variant="ghost"
                            size="sm"
                            className="h-6 px-1.5 text-xs text-muted-foreground hover:text-foreground"
                          >
                            +{remainingCount} more
                          </Button>
                        </DialogTrigger>
                        <DialogContent className="sm:max-w-sm">
                          <DialogHeader>
                            <DialogTitle>Exposure countries</DialogTitle>
                          </DialogHeader>
                          <ul className="max-h-[300px] overflow-y-auto space-y-1.5 text-sm">
                            {exposureCodes.map((code) => (
                              <li key={code} className="flex justify-between gap-2">
                                <span>{codeToName(code)}</span>
                                <span className="text-muted-foreground font-mono text-xs shrink-0">
                                  {code}
                                </span>
                              </li>
                            ))}
                          </ul>
                        </DialogContent>
                      </Dialog>
                    )}
                  </>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="xs:col-span-5 xs:grid-cols-2 grid gap-4 lg:col-span-3">
        {DESC_AND_REQUIREMENTS.map(({ title, content, isList }, index) => (
          <Card key={index} className="gap-3">
            <CardHeader className="flex items-center justify-between">
              <CardTitle className="text-lg">{title}</CardTitle>

              <Dialog>
                <DialogTrigger>
                  <Maximize2 className="text-primary h-4 w-4" />
                </DialogTrigger>

                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>{title}</DialogTitle>
                  </DialogHeader>

                  <div className="bg-muted h-full rounded-lg p-3">
                    {isList ? (
                      <ul className="scrollbar max-h-[400px] overflow-y-auto text-sm break-words space-y-1">
                        {(content as string[]).map((item, idx) => (
                          <li key={idx} className="flex items-start">
                            <span className="text-primary mr-2 mt-1">•</span>
                            <span>{item}</span>
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="scrollbar max-h-[400px] overflow-y-auto text-sm break-words">
                        {content as string}
                      </p>
                    )}
                  </div>
                </DialogContent>
              </Dialog>
            </CardHeader>

            <CardContent className="grow">
              <div className="bg-muted h-full rounded-lg p-3">
                {isList ? (
                  <ul className="line-clamp-6 text-sm break-words space-y-1">
                    {(content as string[]).slice(0, 4).map((item, idx) => (
                      <li key={idx} className="flex items-start">
                        <span className="text-primary mr-2 mt-1">•</span>
                        <span>{item}</span>
                      </li>
                    ))}
                    {(content as string[]).length > 4 && (
                      <li className="text-muted-foreground text-xs">
                        +{(content as string[]).length - 4} more requirements...
                      </li>
                    )}
                  </ul>
                ) : (
                  <p className="line-clamp-6 text-sm break-words">{content as string}</p>
                )}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Job Steps Pipeline Section with View Toggle */}
      {jobPost.assignedSteps && jobPost.assignedSteps.length > 0 && (
        <div className="xs:col-span-5">
          <JobStepsPipelineView 
            steps={jobPost.assignedSteps} 
            candidates={candidates}
            jobPostName={jobPost.name}
            jobPostVersion={jobPost.version}
            onRefreshNeeded={onRefreshNeeded}
          />
        </div>
      )}
    </div>
  );
}
