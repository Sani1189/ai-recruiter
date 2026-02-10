import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { InterviewView } from "@/types/v2/type.view";

export default function MinimumRequirements({
  interview,
}: {
  interview: InterviewView;
}) {
  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="text-lg">Minimum Requirements</CardTitle>
      </CardHeader>

      <CardContent className="space-y-2">
        {interview.jobPost.minimumRequirements.map((req, index) => (
          <p key={index} className="text-muted-foreground text-sm">
            • {req}
          </p>
        ))}
      </CardContent>
    </Card>
  );
}
