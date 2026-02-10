import { ArrowRight, Briefcase, Star, UserCircle } from "lucide-react";
import Link from "next/link";

import { Badge } from "@/components/ui/badge";

import type { UserType } from "@/hooks/useUnifiedAuth";
import { cn } from "@/lib/utils";

export default function Hero() {
  return (
    <section className="container py-10 text-center">
      <Badge variant="secondary" className="border-primary/15 mb-5 border">
        <Star className="mr-2 h-4 w-4" />
        Join 10,000+ Candidates
      </Badge>

      <h1 className="mb-18 text-4xl font-bold tracking-tight md:text-6xl lg:text-7xl">
        AI-Powered
        <span className="from-brand via-brand-secondary to-brand-tertiary block bg-gradient-to-r bg-clip-text text-transparent">
          Voice Interviews
        </span>
      </h1>

      <div className="mx-auto grid grid-cols-1 gap-6 md:grid-cols-2">
        {/* Candidate Box */}
        <CustomerCard
          userType="candidate"
          description="Practice interviews, get AI feedback, and land your dream job with confidence."
          href="/sign-in"
        />

        {/* Recruiter Box */}
        <CustomerCard
          userType="recruiter"
          description="Create AI interviews, screen candidates efficiently, and hire top talent faster."
          href="/recruiter-sign-in"
        />
      </div>
    </section>
  );
}

type CustomerCardProps = {
  userType: Required<UserType>;
  description: string;
  href: string;
};
const CustomerCard = ({ description, userType, href }: CustomerCardProps) => {
  const Icon = userType === "candidate" ? UserCircle : Briefcase;

  return (
    <Link
      href={href}
      className="group from-background/80 to-background/40 hover:border-brand/50 focus:border-brand/50 hover:shadow-brand/10 focus:shadow-brand/10 relative overflow-hidden rounded-2xl border bg-gradient-to-br p-8 shadow-sm backdrop-blur-xl transition-all duration-300 hover:shadow-lg focus:shadow-lg"
    >
      <div
        className={cn(
          "absolute inset-0 bg-gradient-to-br via-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100 group-focus:opacity-100",
          {
            "from-brand/10 to-brand-secondary/10": userType === "candidate",
            "from-brand-secondary/10 to-brand-tertiary/10":
              userType === "recruiter",
          },
        )}
      />

      <div className="relative">
        <div
          className={cn(
            "flex h-14 w-14 items-center justify-center rounded-xl bg-gradient-to-br transition-transform duration-300 group-hover:scale-110",
            {
              "from-brand/20 to-brand-secondary/20": userType === "candidate",
              "from-brand-secondary/20 to-brand-tertiary/20":
                userType === "recruiter",
            },
          )}
        >
          <Icon
            className={cn("h-7 w-7", {
              "text-brand": userType === "candidate",
              "text-brand-secondary": userType === "recruiter",
            })}
          />
        </div>
        <h3 className="mb-2 text-2xl font-bold capitalize">{userType}</h3>
        <p className="text-muted-foreground mb-4">{description}</p>
        <span className="text-brand inline-flex items-center font-medium transition-all group-hover:gap-2">
          Get Started <ArrowRight className="ml-1 h-4 w-4" />
        </span>
      </div>
    </Link>
  );
};
