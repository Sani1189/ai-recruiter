import { Brain, Clock, Mic, Shield, Star, Users } from "lucide-react";

import { Card, CardContent } from "@/components/ui/card";

const FEATURES = [
  {
    title: "Voice Interviews",
    description:
      "Conduct natural voice conversations with AI that feels human-like and engaging.",
    icon: Mic,
    gradient: "from-brand to-brand-secondary",
  },
  {
    title: "AI Analysis",
    description:
      "Get detailed insights and scoring based on responses, skills, and performance.",
    icon: Brain,
    gradient: "from-brand-secondary to-brand-tertiary",
  },
  {
    title: "Easy Sharing",
    description:
      "Generate unique links to share with candidates for seamless interview access.",
    icon: Users,
    gradient: "from-brand-tertiary to-brand-accent",
  },
  {
    title: "Time Efficient",
    description:
      "Save hours of manual screening with automated interview processes.",
    icon: Clock,
    gradient: "from-brand to-brand-secondary",
  },
  {
    title: "Consistent Quality",
    description:
      "Ensure fair and standardized evaluation across all candidates.",
    icon: Star,
    gradient: "from-brand-secondary to-brand-tertiary",
  },
  {
    title: "Secure & Private",
    description:
      "Enterprise-grade security ensures candidate data protection and privacy.",
    icon: Shield,
    gradient: "from-brand-tertiary to-brand-accent",
  },
];

export default function Features() {
  return (
    <section className="bg-muted/30 py-20">
      <div className="container space-y-16">
        <div className="space-y-4 text-center">
          <h2 className="text-3xl font-bold md:text-4xl">
            Why Choose InterviewAI?
          </h2>

          <p className="text-muted-foreground mx-auto max-w-2xl text-lg">
            Streamline your hiring process with cutting-edge AI technology
          </p>
        </div>

        <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
          {FEATURES.map((feature, index) => (
            <Card
              key={index}
              className="shadow-card border-0 transition-all duration-300 hover:shadow-xl"
            >
              <CardContent className="p-8 text-center">
                <div
                  className={`mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-r ${feature.gradient}`}
                >
                  <feature.icon className="h-8 w-8 text-white" />
                </div>

                <h3 className="mb-3 text-xl font-semibold">{feature.title}</h3>
                <p className="text-muted-foreground">{feature.description}</p>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </section>
  );
}
