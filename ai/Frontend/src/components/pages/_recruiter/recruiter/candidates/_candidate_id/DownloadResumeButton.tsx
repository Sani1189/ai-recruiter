import { Download, File, FileText, Headphones } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function DownloadReviewOptions() {
  return (
    <Card className="lg:col-span-3">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <File className="text-brand h-5 w-5" />
          Download & Review Options
        </CardTitle>
      </CardHeader>

      <CardContent className="flex flex-wrap gap-4">
        <Button
          variant="outline"
          // onClick={() => window.open(candidate.resumeUrl, "_blank")}
        >
          <Download className="mr-2 h-4 w-4" />
          Download Resume
        </Button>

        <Button
          variant="outline"
          // onClick={() => window.open(candidate.transcriptUrl, "_blank")}
        >
          <FileText className="mr-2 h-4 w-4" />
          View Interview Transcript
        </Button>

        <Button
          variant="outline"
          // onClick={() => window.open(candidate.audioUrl, "_blank")}
        >
          <Headphones className="mr-2 h-4 w-4" />
          Listen to Interview Audio
        </Button>
      </CardContent>
    </Card>
  );
}
