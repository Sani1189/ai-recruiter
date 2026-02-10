import JobDescription from "./JobDescription";
import MinimumRequirements from "./MinimumRequirements";
import Tips from "./Tips";

import { InterviewView } from "@/types/v2/type.view";

export default function Sidebar({ interview }: { interview: InterviewView }) {
  return (
    <div className="space-y-6">
      <JobDescription jobDescription={interview.jobPost.jobDescription} />
      <MinimumRequirements interview={interview} />
      <Tips />
    </div>
  );
}
