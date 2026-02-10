"use client";

import {
  BarChart3,
  Briefcase,
  ChevronLeft,
  ChevronRight,
  ClipboardList,
  FileText,
  HandCoins,
  Home,
  LogOut,
  Mail,
  Menu,
  Plus,
  Settings,
  User,
  Users,
  X,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ThemeSwitcher } from "./ThemeSwitcher";

import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/useAuthStore";

const MAIN_LINKS = [
  {
    href: "/recruiter/dashboard",
    icon: Home,
    label: "Dashboard",
    section: "main",
  },
  {
    href: "/recruiter/jobs/new",
    icon: Plus,
    label: "New Job",
    section: "main",
  },
];

const PRIMARY_LINKS = [
  {
    href: "/recruiter/jobs",
    icon: Briefcase,
    label: "Jobs",
    section: "primary",
  },
  {
    href: "/recruiter/candidates",
    icon: Users,
    label: "Candidates",
    section: "primary",
  },
  {
    href: "/recruiter/jobSteps",
    icon: ClipboardList,
    label: "Job Steps",
    section: "primary",
  },
  {
    href: "/recruiter/assessmentTemplates",
    icon: FileText,
    label: "Questionnaire Templates",
    section: "primary",
  },
  {
    href: "/recruiter/prompts",
    icon: FileText,
    label: "Prompts",
    section: "primary",
  },
];

const TENANT_LINKS = [
  {
    href: "/recruiter/tenants/profiles",
    icon: User,
    label: "Profile",
    section: "tenant",
  },
  {
    href: "/recruiter/tenants/users",
    icon: Users,
    label: "Users List",
    section: "tenant",
  },
  {
    href: "/recruiter/tenants/services",
    icon: HandCoins,
    label: "Services List",
    section: "tenant",
  },
  {
    href: "/recruiter/tenants/email-setup",
    icon: Mail,
    label: "Email Setup",
    section: "tenant",
  },
];

const SETTINGS_LINKS = [
  {
    href: "/recruiter/interviewConfigurations",
    icon: Settings,
    label: "Interview Configs",
    section: "settings",
  },
  {
    href: "/recruiter/profile",
    icon: User,
    label: "Profile",
    section: "settings",
  },
];

export default function RecruiterSidebar() {
  const [isExpanded, setIsExpanded] = useState(true);
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const pathname = usePathname();
  const { user: currentUser } = useAuthStore();
  const { logout } = useUnifiedAuth();

  // Close mobile menu on route change
  useEffect(() => {
    setIsMobileOpen(false);
  }, [pathname]);

  // Close sidebar on smaller screens
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 1024) {
        setIsExpanded(false);
      }
    };

    window.addEventListener("resize", handleResize);
    handleResize();
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  const handleLogout = async () => {
    await logout();
  };

  const isActive = (href: string) => {
    return pathname.startsWith(href.split("/").slice(0, 4).join("/"));
  };

  const NavLink = ({
    href,
    icon: Icon,
    label,
    showLabel = true,
  }: {
    href: string;
    icon: React.FC<{ className?: string }>;
    label: string;
    showLabel?: boolean;
  }) => {
    const active = isActive(href);
    return (
      <Link href={href}>
        <div
          className={cn(
            "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200",
            active
              ? "bg-sidebar-primary text-sidebar-primary-foreground shadow-md"
              : "text-sidebar-foreground hover:bg-sidebar-primary/20 hover:text-sidebar-primary",
          )}
        >
          <Icon className="h-5 w-5 flex-shrink-0" />
          {showLabel && <span className="truncate">{label}</span>}
          {!showLabel && <span className="sr-only">{label}</span>}
        </div>
      </Link>
    );
  };

  return (
    <>
      {/* Mobile Header */}
      <div className="border-sidebar-border bg-sidebar sticky top-0 z-40 flex items-center justify-between border-b px-4 py-3 lg:hidden">
        <Link href="/recruiter/dashboard" className="flex items-center gap-2">
          <div className="from-brand to-brand-secondary grid h-8 w-8 place-content-center rounded-lg bg-gradient-to-r">
            {/* Logo Icon */}
          </div>
          <span className="text-sidebar-foreground font-semibold">
            InterviewAI
          </span>
        </Link>
        <div className="flex items-center gap-2">
          <ThemeSwitcher />
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setIsMobileOpen(!isMobileOpen)}
            className="text-sidebar-foreground"
          >
            {isMobileOpen ? (
              <X className="h-5 w-5" />
            ) : (
              <Menu className="h-5 w-5" />
            )}
          </Button>
        </div>
      </div>

      {/* Mobile Overlay */}
      {isMobileOpen && (
        <div
          className="fixed inset-0 z-30 bg-black/20 lg:hidden"
          onClick={() => setIsMobileOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          "border-sidebar-border bg-sidebar fixed top-0 bottom-0 left-0 z-40 flex h-screen flex-col border-r transition-all duration-300 lg:relative lg:translate-x-0",
          isExpanded ? "w-72" : "w-24",
          isMobileOpen
            ? "w-72 translate-x-0"
            : "-translate-x-full lg:translate-x-0",
        )}
      >
        {/* Sidebar Header */}
        <div className="border-sidebar-border hidden border-b px-4 py-6 lg:flex lg:items-center lg:justify-between">
          <Link
            href="/recruiter/dashboard"
            className="flex items-center gap-3 transition-opacity hover:opacity-80"
          >
            <div className="from-brand to-brand-secondary grid h-10 w-10 flex-shrink-0 place-content-center rounded-lg bg-gradient-to-r">
              <BarChart3 className="h-6 w-6 text-white" />
            </div>
            {isExpanded && (
              <div className="flex-1">
                <div className="text-sidebar-foreground text-sm font-bold">
                  InterviewAI
                </div>
                <div className="text-sidebar-foreground/60 text-xs">
                  Recruiter
                </div>
              </div>
            )}
          </Link>
          <div className="flex items-center gap-1">
            <ThemeSwitcher />
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setIsExpanded(!isExpanded)}
              className="text-sidebar-foreground hover:bg-sidebar-accent/50"
            >
              {isExpanded ? (
                <ChevronLeft className="h-4 w-4" />
              ) : (
                <ChevronRight className="h-4 w-4" />
              )}
            </Button>
          </div>
        </div>

        {/* Scrollable Content */}
        <div className="scrollbar flex-1 overflow-auto">
          {/* Main Section */}
          <div className="space-y-2 px-3 py-6">
            {MAIN_LINKS.map((link, idx) => (
              <NavLink
                key={idx}
                {...link}
                showLabel={isExpanded || isMobileOpen}
              />
            ))}
          </div>

          {/* Divider */}
          <div className="border-sidebar-border/50 mx-3 border-t" />

          {/* Primary Section */}
          <div className="space-y-2 px-3 py-4">
            {(isExpanded || isMobileOpen) && (
              <div className="text-sidebar-foreground/60 px-3 py-2 text-xs font-semibold tracking-wider uppercase">
                Recruitment
              </div>
            )}
            {PRIMARY_LINKS.map((link, idx) => (
              <NavLink
                key={idx}
                {...link}
                showLabel={isExpanded || isMobileOpen}
              />
            ))}
          </div>

          {/* Divider */}
          <div className="border-sidebar-border/50 mx-3 border-t" />

          {/* Primary Section */}
          <div className="space-y-2 px-3 py-4">
            {(isExpanded || isMobileOpen) && (
              <div className="text-sidebar-foreground/60 px-3 py-2 text-xs font-semibold tracking-wider uppercase">
                Tenant
              </div>
            )}

            {TENANT_LINKS.map((link, idx) => (
              <NavLink
                key={idx}
                {...link}
                showLabel={isExpanded || isMobileOpen}
              />
            ))}
          </div>

          {/* Divider */}
          <div className="border-sidebar-border/50 mx-3 border-t" />

          {/* Settings Section */}
          <div className="space-y-2 px-3 py-4">
            {(isExpanded || isMobileOpen) && (
              <div className="text-sidebar-foreground/60 px-3 py-2 text-xs font-semibold tracking-wider uppercase">
                Settings
              </div>
            )}
            {SETTINGS_LINKS.map((link, idx) => (
              <NavLink
                key={idx}
                {...link}
                showLabel={isExpanded || isMobileOpen}
              />
            ))}
          </div>
        </div>

        {/* User Profile Footer */}
        <div className="border-sidebar-border border-t px-3 py-4">
          {currentUser ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  className={cn(
                    "w-full justify-start gap-3 rounded-lg transition-colors",
                    isExpanded || isMobileOpen ? "px-3" : "px-2",
                    "hover:bg-sidebar-primary/20 hover:text-sidebar-primary",
                  )}
                >
                  <Avatar className="border-sidebar-border h-8 w-8 flex-shrink-0 border">
                    <AvatarImage
                      src={currentUser.profilePictureUrl ?? undefined}
                    />
                    <AvatarFallback className="text-xs">
                      {currentUser.name?.charAt(0).toUpperCase() || "U"}
                    </AvatarFallback>
                  </Avatar>
                  {(isExpanded || isMobileOpen) && (
                    <div className="flex-1 overflow-hidden text-left">
                      <div className="text-sidebar-foreground truncate text-sm font-medium">
                        {currentUser.name?.split(" ")[0] || "User"}
                      </div>
                      <div className="text-sidebar-foreground/60 truncate text-xs">
                        {currentUser.email?.split("@")[0] || "Recruiter"}
                      </div>
                    </div>
                  )}
                </Button>
              </DropdownMenuTrigger>

              <DropdownMenuContent
                align={isExpanded || isMobileOpen ? "end" : "start"}
                className="w-56"
              >
                <div className="px-2 py-1.5">
                  <p className="text-sidebar-foreground text-xs font-medium">
                    {currentUser.name}
                  </p>
                  <p className="text-sidebar-foreground/60 text-xs">
                    {currentUser.email}
                  </p>
                </div>

                <DropdownMenuSeparator />

                <DropdownMenuItem asChild className="cursor-pointer">
                  <Link href="/recruiter/profile">
                    <User className="mr-2 h-4 w-4" />
                    <span>Profile</span>
                  </Link>
                </DropdownMenuItem>

                <DropdownMenuSeparator />

                <DropdownMenuItem
                  onClick={handleLogout}
                  className="text-destructive focus:bg-destructive/10 cursor-pointer"
                >
                  <LogOut className="mr-2 h-4 w-4" />
                  <span>Logout</span>
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : null}
        </div>
      </aside>
    </>
  );
}
