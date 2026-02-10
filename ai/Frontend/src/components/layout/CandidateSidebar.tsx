"use client"

import type React from "react"

import { useState, useEffect } from "react"
import {
  Home,
  ChevronLeft,
  ChevronRight,
  LogOut,
  User,
  Menu,
  X,
  Briefcase,
  Bell,
  BarChart3,
  BombIcon as JobIcon,
} from "lucide-react"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { useAuthStore } from "@/stores/useAuthStore"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { cn } from "@/lib/utils"
import { ThemeSwitcher } from "./ThemeSwitcher"

const MAIN_LINKS = [
  {
    href: "/profile",
    icon: Home,
    label: "Dashboard",
    section: "main",
  },
]

const PRIMARY_LINKS = [
  {
    href: "/jobs",
    icon: JobIcon,
    label: "Browse Jobs",
    section: "primary",
  },
  {
    href: "/applications",
    icon: Briefcase,
    label: "My Applications",
    section: "primary",
  },
  {
    href: "/notifications",
    icon: Bell,
    label: "Notifications",
    section: "primary",
  },
]

const SETTINGS_LINKS = [
  {
    href: "/profile",
    icon: User,
    label: "Profile",
    section: "settings",
  },
]

export default function CandidateSidebar() {
  const [isExpanded, setIsExpanded] = useState(true)
  const [isMobileOpen, setIsMobileOpen] = useState(false)
  const pathname = usePathname()
  const { user: currentUser } = useAuthStore()
  const { logout } = useUnifiedAuth()

  // Close mobile menu on route change
  useEffect(() => {
    setIsMobileOpen(false)
  }, [pathname])

  // Close sidebar on smaller screens
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 1024) {
        setIsExpanded(false)
      }
    }

    window.addEventListener("resize", handleResize)
    handleResize()
    return () => window.removeEventListener("resize", handleResize)
  }, [])

  const handleLogout = async () => {
    await logout()
  }

  const isActive = (href: string) => {
    return pathname === href || pathname.startsWith(href + "/")
  }

  const NavLink = ({
    href,
    icon: Icon,
    label,
    showLabel = true,
  }: {
    href: string
    icon: React.FC<{ className?: string }>
    label: string
    showLabel?: boolean
  }) => {
    const active = isActive(href)
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
    )
  }

  return (
    <>
      {/* Mobile Header */}
      <div className="sticky top-0 z-40 flex items-center justify-between border-b border-sidebar-border bg-sidebar px-4 py-3 lg:hidden">
        <Link href="/profile" className="flex items-center gap-2">
          <div className="grid h-8 w-8 place-content-center rounded-lg bg-gradient-to-r from-brand to-brand-secondary">
            <BarChart3 className="h-5 w-5 text-white" />
          </div>
          <span className="font-semibold text-sidebar-foreground">InterviewAI</span>
        </Link>
        <div className="flex items-center gap-2">
          <ThemeSwitcher />
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setIsMobileOpen(!isMobileOpen)}
            className="text-sidebar-foreground"
          >
            {isMobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
        </div>
      </div>

      {/* Mobile Overlay */}
      {isMobileOpen && (
        <div className="fixed inset-0 z-30 bg-black/20 lg:hidden" onClick={() => setIsMobileOpen(false)} />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          "fixed bottom-0 left-0 top-0 z-40 flex flex-col border-r border-sidebar-border bg-sidebar transition-all duration-300 lg:relative lg:translate-x-0",
          isExpanded ? "w-72" : "w-24",
          isMobileOpen ? "translate-x-0 w-72" : "-translate-x-full lg:translate-x-0",
        )}
      >
        {/* Sidebar Header */}
        <div className="hidden border-b border-sidebar-border px-4 py-6 lg:flex lg:items-center lg:justify-between">
          <Link href="/profile" className="flex items-center gap-3 transition-opacity hover:opacity-80">
            <div className="grid h-10 w-10 place-content-center rounded-lg bg-gradient-to-r from-brand to-brand-secondary flex-shrink-0">
              <BarChart3 className="h-6 w-6 text-white" />
            </div>
            {isExpanded && (
              <div className="flex-1">
                <div className="text-sm font-bold text-sidebar-foreground">InterviewAI</div>
                <div className="text-xs text-sidebar-foreground/60">Candidate</div>
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
              {isExpanded ? <ChevronLeft className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
            </Button>
          </div>
        </div>

        {/* Scrollable Content */}
        <div className="flex-1 overflow-y-auto scrollbar">
          {/* Main Section */}
          <div className="space-y-2 px-3 py-6">
            {MAIN_LINKS.map((link, idx) => (
              <NavLink key={idx} {...link} showLabel={isExpanded || isMobileOpen} />
            ))}
          </div>

          {/* Divider */}
          <div className="mx-3 border-t border-sidebar-border/50" />

          {/* Primary Section */}
          <div className="space-y-2 px-3 py-4">
            {(isExpanded || isMobileOpen) && (
              <div className="px-3 py-2 text-xs font-semibold text-sidebar-foreground/60 uppercase tracking-wider">
                Career
              </div>
            )}
            {PRIMARY_LINKS.map((link, idx) => (
              <NavLink key={idx} {...link} showLabel={isExpanded || isMobileOpen} />
            ))}
          </div>

          {/* Divider */}
          <div className="mx-3 border-t border-sidebar-border/50" />

          {/* Settings Section */}
          <div className="space-y-2 px-3 py-4">
            {(isExpanded || isMobileOpen) && (
              <div className="px-3 py-2 text-xs font-semibold text-sidebar-foreground/60 uppercase tracking-wider">
                Account
              </div>
            )}
            {SETTINGS_LINKS.map((link, idx) => (
              <NavLink key={idx} {...link} showLabel={isExpanded || isMobileOpen} />
            ))}
          </div>
        </div>

        {/* User Profile Footer */}
        <div className="border-t border-sidebar-border px-3 py-4">
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
                  <Avatar className="h-8 w-8 border border-sidebar-border flex-shrink-0">
                    <AvatarImage src={currentUser.profilePictureUrl ?? undefined} />
                    <AvatarFallback className="text-xs">
                      {currentUser.name?.charAt(0).toUpperCase() || "U"}
                    </AvatarFallback>
                  </Avatar>
                  {(isExpanded || isMobileOpen) && (
                    <div className="flex-1 overflow-hidden text-left">
                      <div className="truncate text-sm font-medium text-sidebar-foreground">
                        {currentUser.name?.split(" ")[0] || "User"}
                      </div>
                      <div className="truncate text-xs text-sidebar-foreground/60">
                        {currentUser.email?.split("@")[0] || "Candidate"}
                      </div>
                    </div>
                  )}
                </Button>
              </DropdownMenuTrigger>

              <DropdownMenuContent align={isExpanded || isMobileOpen ? "end" : "start"} className="w-56">
                <div className="px-2 py-1.5">
                  <p className="text-xs font-medium text-sidebar-foreground">{currentUser.name}</p>
                  <p className="text-xs text-sidebar-foreground/60">{currentUser.email}</p>
                </div>

                <DropdownMenuSeparator />

                <DropdownMenuItem asChild className="cursor-pointer">
                  <Link href="/profile">
                    <User className="mr-2 h-4 w-4" />
                    <span>Profile</span>
                  </Link>
                </DropdownMenuItem>

                <DropdownMenuSeparator />

                <DropdownMenuItem
                  onClick={handleLogout}
                  className="cursor-pointer text-destructive focus:bg-destructive/10"
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
  )
}
