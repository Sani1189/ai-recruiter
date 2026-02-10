import { Briefcase, CheckCircle, TrendingUp, Trophy } from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

const FEATURES = [
  {
    icon: Trophy,
    title: "Practice Real Interviews",
    description:
      "Access hundreds of real interview questions from top companies",
    benefits: [
      "Company-specific questions",
      "Multiple difficulty levels",
      "Video & coding challenges",
    ],
  },
  {
    icon: TrendingUp,
    title: "AI-Powered Feedback",
    description: "Get instant, detailed feedback on your performance",
    benefits: [
      "Communication analysis",
      "Technical skill assessment",
      "Improvement suggestions",
    ],
  },
  {
    icon: Briefcase,
    title: "Connect with Employers",
    description: "Showcase your skills and get discovered by recruiters",
    benefits: [
      "Profile visibility",
      "Direct recruiter contact",
      "Interview invitations",
    ],
  },
];

export default function Features() {
  return (
    <section className="container py-20">
      <div className="mb-16 text-center">
        <h2 className="mb-4 text-3xl font-bold">Why Choose Our Platform?</h2>
        <p className="text-muted-foreground text-lg">
          Everything you need to ace your next interview
        </p>
      </div>

      <div className="mb-16 grid gap-8 md:grid-cols-3">
        {FEATURES.map((feature, index) => (
          <Card key={index} className="border-0 text-center shadow-lg">
            <CardHeader>
              <div className="bg-primary/10 mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-lg">
                <feature.icon className="text-primary h-6 w-6" />
              </div>
              <CardTitle>{feature.title}</CardTitle>
              <CardDescription>{feature.description}</CardDescription>
            </CardHeader>

            <CardContent>
              <ul className="text-muted-foreground list-inside space-y-2 text-sm">
                {feature.benefits.map((benefit, index) => (
                  <li key={index} className="flex items-center pl-4">
                    <CheckCircle className="text-primary mr-2 h-4 w-4" />
                    {benefit}
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}
