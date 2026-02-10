"use client"

import { useAuthStore } from "@/stores/useAuthStore"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { BarChart3, LogOut } from "lucide-react"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { ThemeSwitcher } from "./ThemeSwitcher"

export default function JobPostHeader() {
  const { user } = useAuthStore()
  const { logout } = useUnifiedAuth()
  const router = useRouter()

  const handleLogout = async () => {
    await logout()
    router.push("/sign-in")
  }

  const getUserInitials = () => {
    if (!user) return "U"
    if (typeof user === "object" && "name" in user && user.name) {
      return user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
    }
    return "U"
  }

  const getUserName = () => {
    if (!user) return "Guest"
    if (typeof user === "object" && "name" in user) {
      return user.name || "User"
    }
    return "User"
  }

  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container flex items-center justify-between h-16 px-4">
        {/* Logo */}
        <Link href="/" className="flex items-center gap-2 font-semibold">
          <div className="grid h-8 w-8 place-content-center rounded-lg bg-gradient-to-r from-brand to-brand-secondary">
            <BarChart3 className="h-5 w-5 text-white" />
          </div>
          <span className="hidden sm:inline">InterviewAI</span>
        </Link>

        {/* Right Section */}
        <div className="flex items-center gap-4">
          <ThemeSwitcher />

          {/* User Menu */}
          {user ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                  <Avatar className="h-8 w-8">
                    <AvatarImage
                      src={
                        typeof user === "object" && "avatar" in user && (user as any).avatar
                          ? (user as any).avatar
                          : undefined
                      }
                      alt={getUserName()}
                    />
                    <AvatarFallback className="bg-gradient-to-r from-brand to-brand-secondary text-white text-xs font-bold">
                      {getUserInitials()}
                    </AvatarFallback>
                  </Avatar>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <div className="flex items-center justify-start gap-2 p-2">
                  <div className="flex flex-col space-y-1 leading-none">
                    <p className="font-medium text-sm">{getUserName()}</p>
                    <p className="w-full truncate text-xs text-muted-foreground">
                      {typeof user === "object" && "email" in user ? (user as any).email : "candidate"}
                    </p>
                  </div>
                </div>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                  <Link href="/applications">My Applications</Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/profile">Profile</Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleLogout} className="text-destructive">
                  <LogOut className="mr-2 h-4 w-4" />
                  Logout
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Button onClick={() => router.push("/sign-in")} variant="default">
              Sign In
            </Button>
          )}
        </div>
      </div>
    </header>
  )
}
