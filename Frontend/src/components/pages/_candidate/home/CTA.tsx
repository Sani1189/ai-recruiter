"use client";

import { ArrowRight } from "lucide-react";
import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/useAuthStore";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";

export default function CTA() {
  const { user } = useAuthStore();
  
  const isAuthenticated = !!user;

  return (
    <section className="container py-20 text-center">
      <h2 className="mb-6 text-3xl font-bold">Ready to Start Your Journey?</h2>

      <p className="text-muted-foreground mb-8 text-lg">
        Join thousands of candidates who have already improved their interview
        skills and landed their dream jobs.
      </p>

      <div className="flex flex-col justify-center gap-4 sm:flex-row">
        <Link
          href="/sign-up"
          className={cn(buttonVariants({ size: "lg" }), "px-8 text-lg")}
        >
          Create Free Account
          <ArrowRight className="ml-2 h-5 w-5" />
        </Link>

        {!isAuthenticated && (
          <Link
            href="/sign-in"
            className={cn(
              buttonVariants({ variant: "outline", size: "lg" }),
              "px-8 text-lg",
            )}
          >
            Already have an account?
          </Link>
        )}
      </div>
    </section>
  );
}
