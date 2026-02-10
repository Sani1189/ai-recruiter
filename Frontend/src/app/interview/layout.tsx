import type React from "react"
import type { Metadata } from "next"
import PublicJobPostHeader from "@/components/layout/PublicJobPostHeader"
import { Toaster } from "@/components/ui/sonner"

export const metadata: Metadata = {
  title: "Job Application Interview",
  description: "Apply for a job position through AI-powered interview",
}

export default function InterviewLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <>
      <PublicJobPostHeader />
      <main>{children}</main>
      <Toaster />
    </>
  )
}
