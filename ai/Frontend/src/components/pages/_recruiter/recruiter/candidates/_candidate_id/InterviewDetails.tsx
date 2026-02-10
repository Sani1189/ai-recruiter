import { Calendar, Clock } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { CandidateView, InterviewView } from "@/types/v2/type.view";

const formatDuration = (minutes: number) => {
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

export default function InterviewDetails({
  candidate,
  interview,
}: {
  candidate: CandidateView;
  interview?: InterviewView;
}) {
  if (!interview) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Clock className="text-brand h-5 w-5" />
          Last Interview Details
        </CardTitle>
      </CardHeader>

      <CardContent className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="space-y-4">
          <div>
            <label className="text-muted-foreground text-sm font-medium">
              Duration
            </label>

            <p className="text-sm">{formatDuration(interview.duration)}</p>
          </div>

          <div>
            <label className="text-muted-foreground text-sm font-medium">
              Completed At
            </label>

            <div className="flex items-center gap-2">
              <Calendar className="text-brand h-4 w-4" />

              <span className="text-sm">
                {formatDate(interview.completedAt)}
              </span>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <div>
            <label className="text-muted-foreground text-sm font-medium">
              Position
            </label>

            <p className="text-sm">{interview.jobPost.jobTitle}</p>
          </div>

          <div>
            <label className="text-muted-foreground text-sm font-medium">
              Experience Level
            </label>

            <p className="text-sm">{interview.jobPost.experienceLevel}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
