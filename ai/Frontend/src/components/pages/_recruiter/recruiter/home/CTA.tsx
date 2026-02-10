import { ArrowRight } from "lucide-react";
import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export default function CTA() {
  return (
    <section className="py-20">
      <div className="container text-center">
        <h2 className="mb-6 text-3xl font-bold md:text-4xl">
          Ready to Transform Your Hiring?
        </h2>

        <p className="text-muted-foreground mb-8 text-lg">
          Join thousands of companies using AI to hire better, faster, and more
          efficiently.
        </p>

        <Link
          href="/recruiter/jobs/new"
          className={cn(buttonVariants({ size: "lg" }), "px-8 text-lg")}
        >
          Get Started Now
          <ArrowRight className="ml-2 h-5 w-5" />
        </Link>
      </div>
    </section>
  );
}
