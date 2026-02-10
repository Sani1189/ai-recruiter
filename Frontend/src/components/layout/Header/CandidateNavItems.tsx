import { Menu, Files } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";

import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/useAuthStore";

const LINKS = [
  {
    href: "/applications",
    icon: Files,
    label: "My Applications",
  },
];

export default function CandidateNavItems() {
  const { user, userType } = useAuthStore();
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  // Update authentication state when store changes
  useEffect(() => {
    const authStatus = !!user && userType === 'candidate';
    setIsAuthenticated(authStatus);
  }, [user, userType]);

  // Don't render navigation items if user is not authenticated as a candidate
  if (!isAuthenticated) {
    return null;
  }

  return (
    <>
      <NavItems />

      <Sheet>
        <SheetTrigger className="flex md:hidden">
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
          "hidden items-center md:flex": variant === "desktop",
        },
        className,
      )}
    >
      {LINKS.map((link, index) => (
        <Link
          key={index}
          href={link.href}
          className={cn(buttonVariants({ variant: "ghost", size: "sm" }), {
            "bg-accent justify-start": variant === "mobile",
          })}
        >
          <link.icon className="mr-2 h-4 w-4" />
          {link.label}
        </Link>
      ))}
    </nav>
  );
};


