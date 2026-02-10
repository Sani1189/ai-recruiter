import {
  BarChart3,
  Briefcase,
  ClipboardList,
  FileText,
  Menu,
  Plus,
  Settings,
  Users,
} from "lucide-react";
import Link from "next/link";

import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const LINKS = [
  {
    href: "/recruiter/jobs/new",
    icon: Plus,
    label: "New Job Opening",
  },
  {
    href: "/recruiter/dashboard",
    icon: BarChart3,
    label: "Dashboard",
  },
  {
    href: "/recruiter/jobs",
    icon: Briefcase,
    label: "Jobs",
  },
  {
    href: "/recruiter/candidates",
    icon: Users,
    label: "Candidates",
  },
  {
    href: "/recruiter/jobSteps",
    icon: ClipboardList,
    label: "Job Steps",
  },
  {
    href: "/recruiter/prompts",
    icon: FileText,
    label: "Prompts",
  },
  {
    href: "/recruiter/interviewConfigurations",
    icon: Settings,
    label: "Interview Configs",
  },
];

export default function RecruiterNavItems() {
  return (
    <>
      <NavItems />

      <Sheet>
        <SheetTrigger className="flex xl:hidden">
          <Menu className="text-foreground h-4 w-4" />
        </SheetTrigger>

        <SheetContent>
          <SheetHeader>
            <SheetTitle className="sr-only">Navigation</SheetTitle>
          </SheetHeader>

          <NavItems variant="mobile" />
        </SheetContent>
      </Sheet>
    </>
  );
}

const NavItems = ({
  className,
  variant = "desktop",
}: {
  className?: string;
  variant?: "desktop" | "mobile";
}) => {
  return (
    <nav
      className={cn(
        "flex gap-2",
        {
          "flex-col px-3": variant === "mobile",
          "hidden items-center xl:flex": variant === "desktop",
        },
        className,
      )}
    >
      {LINKS.map((link, index) => (
        <Link
          key={index}
          href={link.href}
          className={cn(buttonVariants({ variant: "ghost", size: "sm" }), {
            "bg-accent/50 border-brand-secondary/15 h-auto justify-start border py-2":
              variant === "mobile",
          })}
        >
          <link.icon className="mr-2 h-4 w-4" />
          {link.label}
        </Link>
      ))}
    </nav>
  );
};
