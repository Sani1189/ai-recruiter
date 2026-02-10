import { Calendar, FileText } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { CandidateView } from "@/types/v2/type.view";

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

export default function ApplicationDetails({
  candidate,
  application,
}: {
  candidate: CandidateView;
  application?: any;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileText className="text-brand h-5 w-5" />
          Last Application Details
        </CardTitle>
      </CardHeader>

      <CardContent className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {!application ? (
          <div className="col-span-full">
            <p className="text-center text-muted-foreground text-sm">
              Not applied yet
            </p>
          </div>
        ) : (
          <>
            <div className="space-y-4">
              <div>
                <label className="text-muted-foreground text-sm font-medium">
                  Applied Date
                </label>

                <div className="flex items-center gap-2">
                  <Calendar className="text-brand h-4 w-4" />

                  <span className="text-sm">
                    {application.completedAt ? formatDate(application.completedAt) : "N/A"}
                  </span>
                </div>
              </div>

              <div>
                <label className="text-muted-foreground text-sm font-medium">
                  Job Position
                </label>

                <p className="text-sm">{application.jobPost.jobTitle}</p>
              </div>
            </div>

            <div className="space-y-4">
              <div>
                <label className="text-muted-foreground text-sm font-medium">
                  Job Type
                </label>

                <p className="text-sm">{application.jobPost.jobType || "N/A"}</p>
              </div>

              <div>
                <label className="text-muted-foreground text-sm font-medium">
                  Experience Level
                </label>

                <p className="text-sm">{application.jobPost.experienceLevel || "N/A"}</p>
              </div>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

