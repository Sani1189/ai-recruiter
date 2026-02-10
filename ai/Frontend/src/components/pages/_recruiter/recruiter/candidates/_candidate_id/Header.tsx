import { Star } from "lucide-react";

import BackButton from "@/components/ui/back-button";
import { Badge } from "@/components/ui/badge";

import { cn } from "@/lib/utils";
import { CandidateView } from "@/types/v2/type.view";
import { getScoreColor } from "./lib/utils";

export default function Header({
  candidate,
}: {
  candidate: CandidateView;
}) {
  return (
    <div>
      <BackButton />

      <div className="mb-2 flex items-center gap-4">
        <h1 className="from-brand to-brand-secondary bg-gradient-to-r bg-clip-text text-3xl font-bold text-transparent">
          {candidate.userProfile.name}
        </h1>

        <Badge className={cn(getScoreColor(candidate.score.avg), "text-white")}>
          <Star className="mr-1 h-3 w-3" />
          {candidate.score.avg}/10
        </Badge>
      </div>
    </div>
  );
}
