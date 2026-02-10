import type React from "react"
import type { Metadata } from "next"
import { Toaster } from "@/components/ui/sonner"
import PublicJobPostHeader from "@/components/layout/PublicJobPostHeader"

export const metadata: Metadata = {
  title: "Job Details",
  description: "View job posting details and apply",
}

export default function JobPostLayout({
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
