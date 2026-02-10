import type React from "react"
import type { Metadata } from "next"
import { Geist, Geist_Mono } from "next/font/google"
import RecruiterSidebar from "@/components/layout/RecruiterSidebar"
import { Toaster } from "@/components/ui/sonner"
import { UnifiedGuard } from "@/components/auth/UnifiedGuard"

import "@/styles/globals.css"

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
})

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
})

export const metadata: Metadata = {
  title: "Recruiter Dashboard",
  description: "Recruiter administration area",
}

export default function RecruiterLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <UnifiedGuard requiredUserType="recruiter" fallbackPath="/recruiter-sign-in">
      <div className="flex min-h-screen flex-col lg:flex-row">
        <RecruiterSidebar />
        <main className="flex-1 overflow-auto">{children}</main>
      </div>
      <Toaster />
    </UnifiedGuard>
  )
}
