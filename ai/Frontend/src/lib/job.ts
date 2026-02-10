import { toast } from "sonner";

import { getOrigin } from "./utils";

// Generate interview link using job post name and version
function copyInterviewLink(jobPostName: string, version: number) {
  navigator.clipboard.writeText(getInterviewLink(jobPostName, version));
  toast.success("Interview link copied to clipboard!");
}

function getInterviewLink(jobPostName: string, version: number) {
  return `${getOrigin()}/interview/${jobPostName}/${version}`;
}

export { copyInterviewLink, getInterviewLink };
