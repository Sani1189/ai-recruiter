import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const TIPS = [
  "Speak clearly and at a normal pace",
  "Take your time to think before answering",
  "Use specific examples when possible",
  "Ask for clarification if needed",
];

export default function Tips() {
  return (
    <Card className="shadow-card border-brand/20">
      <CardHeader>
        <CardTitle className="text-brand text-lg">Interview Tips</CardTitle>
      </CardHeader>

      <CardContent className="space-y-2">
        {TIPS.map((tip, index) => (
          <p key={index} className="text-muted-foreground text-sm">
            • {tip}
          </p>
        ))}
      </CardContent>
    </Card>
  );
}
