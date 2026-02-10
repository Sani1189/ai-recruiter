import { CircleAlert, Hammer } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { InterviewView } from "@/types/v2/type.view";

export default function Weaknesses({
  interview,
}: {
  interview?: InterviewView;
}) {
  const hasWeaknesses = interview && interview.feedback?.weaknesses?.length > 0;

  return (
    <Card className="border-yellow-500">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Hammer className="h-5 w-5 text-yellow-500" />
          Weaknesses
        </CardTitle>
      </CardHeader>

      <CardContent>
        {hasWeaknesses ? (
          <ul className="list-inside space-y-2 pl-5">
            {interview.feedback.weaknesses.map((weakness, index) => (
              <li key={index} className="text-sm">
                <CircleAlert className="mr-2 inline-block h-4 w-4 text-yellow-500" />
                <span>{weakness}</span>
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-center text-muted-foreground text-sm">
            No weaknesses yet
          </p>
        )}
      </CardContent>
    </Card>
  );
}
