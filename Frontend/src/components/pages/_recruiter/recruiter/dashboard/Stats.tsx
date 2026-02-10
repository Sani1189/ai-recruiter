import { Clock, Star, Users } from "lucide-react";
import Link from "next/link";

import { Card, CardContent } from "@/components/ui/card";
import { DashboardStats } from "@/lib/api/services/dashboard.service";

export default function Stats({ stats }: { stats: DashboardStats }) {
  const STATS = [
    {
      icon: Users,
      value: stats.totalJobOpenings,
      label: "Total Job Openings",
      bgColor: "bg-brand/10",
      textColor: "text-brand",
      href: "/recruiter/jobs",
    },
    {
      icon: Users,
      value: stats.totalCandidates,
      label: "Total Candidates",
      bgColor: "bg-brand-secondary/10",
      textColor: "text-brand-secondary",
      href: "/recruiter/candidates",
    },
    {
      icon: Star,
      value: Math.round(stats.avgOverallScore),
      label: "Avg Overall Score",
      bgColor: "bg-brand-tertiary/10",
      textColor: "text-brand-tertiary",
    },
    {
      icon: Clock,
      value: Math.round(stats.avgDurationMinutes),
      label: "Avg Duration (min)",
      bgColor: "bg-brand-accent/20",
      textColor: "text-brand",
    },
  ];

  return (
    <div className="xs:grid-cols-2 grid gap-6 md:grid-cols-4">
      {STATS.map((stat, index) => {
        const content = (
          <Card className={`shadow-card py-0 ${stat.href ? "cursor-pointer transition-all hover:shadow-lg hover:scale-[1.02]" : ""}`}>
            <CardContent className="flex items-center gap-4 p-6">
              <div
                className={`flex h-12 w-12 items-center justify-center rounded-full ${stat.bgColor}`}
              >
                <stat.icon className={`h-6 w-6 ${stat.textColor}`} />
              </div>

              <div>
                <p className="text-2xl font-bold">{stat.value}</p>
                <p className="text-muted-foreground text-sm">{stat.label}</p>
              </div>
            </CardContent>
          </Card>
        );

        return stat.href ? (
          <Link key={index} href={stat.href}>
            {content}
          </Link>
        ) : (
          <div key={index}>{content}</div>
        );
      })}
    </div>
  );
}
