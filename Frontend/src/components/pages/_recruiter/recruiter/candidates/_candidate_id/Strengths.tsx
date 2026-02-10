import { Check, Lightbulb } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { InterviewView } from "@/types/v2/type.view";

export default function Strengths({ interview }: { interview?: InterviewView }) {
  const hasStrengths = interview && interview.feedback?.strengths?.length > 0;

  return (
    <Card className="border-green-500">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Lightbulb className="h-5 w-5 text-green-500" />
          Strengths
        </CardTitle>
      </CardHeader>

      <CardContent>
        {hasStrengths ? (
          <ul className="list-inside space-y-2 pl-5">
            {interview.feedback.strengths.map((strength, index) => (
              <li key={index} className="text-sm">
                <Check className="mr-2 inline-block h-4 w-4 text-green-500" />
                <span>{strength}</span>
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-center text-muted-foreground text-sm">
            No strengths yet
          </p>
        )}
      </CardContent>
    </Card>
  );
}
