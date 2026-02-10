import { Badge } from "@/components/ui/badge";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Briefcase } from "lucide-react";

export default function InterviewHeader({
  jobTitle,
  jobType,
  experienceLevel,
  tone,
  focusArea,
}: {
  jobTitle: string;
  jobType: string;
  experienceLevel: string;
  tone?: string;
  focusArea?: string;
}) {
  return (
    <Card className="shadow-card">
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="mb-2 text-2xl">{jobTitle}</CardTitle>

            <div className="flex flex-wrap items-center gap-2">
              <Badge variant="secondary">
                <Briefcase className="mr-1 h-3 w-3" />
                {jobType}
              </Badge>
              <Badge variant="outline">{experienceLevel}</Badge>
              {tone && <Badge variant="outline">{tone}</Badge>}
              {focusArea && <Badge variant="outline">{focusArea}</Badge>}
            </div>
          </div>
        </div>
      </CardHeader>
    </Card>
  );
}