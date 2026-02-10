import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function JobDescription({
  jobDescription,
}: {
  jobDescription: string;
}) {
  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="text-lg">Job Description</CardTitle>
      </CardHeader>

      <CardContent>
        <p className="text-muted-foreground text-sm leading-relaxed">
          {jobDescription}
        </p>
      </CardContent>
    </Card>
  );
}
