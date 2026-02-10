import { ArrowRight } from "lucide-react";
import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export default function Hero() {
  return (
    <section className="relative py-20 lg:py-32">
      <div className="from-brand/5 via-brand-secondary/5 to-brand-tertiary/10 absolute inset-0 bg-gradient-to-br" />

      <div className="relative container text-center">
        <h1 className="mb-8 text-4xl font-bold tracking-tight md:text-6xl lg:text-7xl">
          AI-Powered
          <span className="from-brand via-brand-secondary to-brand-tertiary block bg-gradient-to-r bg-clip-text text-transparent">
            Voice Interviews
          </span>
        </h1>

        <p className="text-muted-foreground mx-auto mb-12 max-w-2xl text-lg leading-relaxed md:text-xl">
          Revolutionize your hiring process with intelligent voice interviews.
          Create, conduct, and analyze interviews with AI precision.
        </p>

        <div className="flex flex-col justify-center gap-4 sm:flex-row">
          <Link
            href="/recruiter/jobs/new"
            className={cn(buttonVariants({ size: "lg" }), "px-8 text-lg")}
          >
            New Job Openings
            <ArrowRight className="ml-2 h-5 w-5" />
          </Link>

          <Link
            href="/recruiter/dashboard"
            className={cn(
              buttonVariants({ variant: "outline", size: "lg" }),
              "px-8 text-lg",
            )}
          >
            View Dashboard
          </Link>
        </div>
      </div>
    </section>
  );
}
