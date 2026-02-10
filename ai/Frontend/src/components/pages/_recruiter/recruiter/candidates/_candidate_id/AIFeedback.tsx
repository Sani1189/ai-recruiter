import { Star } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { InterviewView } from "@/types/v2/type.view";

export default function AIFeedback({
  interview,
}: {
  interview?: InterviewView;
}) {
  const hasFeedback = interview && interview.feedback;

  return (
    <Card className="lg:col-span-3">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Star className="text-brand h-5 w-5" />
          AI Feedback
        </CardTitle>
      </CardHeader>

      <CardContent className="flex flex-col gap-6 *:h-full">
        {hasFeedback ? (
          <>
            <div className="flex flex-col gap-2">
              <label className="text-muted-foreground block text-sm font-medium">
                Positive Summary
              </label>

              <p className="bg-muted/50 grow rounded-lg p-4 text-sm leading-relaxed">
                {interview.feedback.summary}
              </p>
            </div>

            <div className="flex flex-col gap-2">
              <label className="text-muted-foreground block text-sm font-medium">
                Negative Summary
              </label>

              <p className="bg-muted/50 grow rounded-lg p-4 text-sm leading-relaxed">
                {interview.feedback.detailed}
              </p>
            </div>
          </>
        ) : (
          <p className="text-center text-muted-foreground text-sm">
            No AI feedback yet
          </p>
        )}
      </CardContent>
    </Card>
  );
}
