"use client"

import { useEffect, useState } from "react"
import Header from "@/components/pages/_recruiter/recruiter/dashboard/Header"
import Stats from "@/components/pages/_recruiter/recruiter/dashboard/Stats"
import { dashboardService, type DashboardStats } from "@/lib/api/services/dashboard.service"
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth"

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { getAccessToken, isLoading: authLoading, user, isAuthenticated } = useUnifiedAuth()

  useEffect(() => {
    if (authLoading || !isAuthenticated) {
      setLoading(false)
      return
    }

    const loadStats = async () => {
      try {
        setLoading(true)
        const token = await getAccessToken()
        if (!token) {
          setError("Authentication required")
          setLoading(false)
          return
        }

        const response = await dashboardService.getDashboardStats(token)
        const data = (response as any)?.data || response
        setStats(data)
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : "Failed to load statistics"
        console.error("Dashboard load error:", errorMsg)

        // Only show error if not an auth error
        if (!errorMsg.includes("Authentication") && !errorMsg.includes("authenticated")) {
          setError(errorMsg)
        }
      } finally {
        setLoading(false)
      }
    }

    loadStats()
  }, [getAccessToken, authLoading, isAuthenticated])

  return (
    <div className="container space-y-8 py-8">
      <Header />
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <p className="text-muted-foreground">Loading dashboard statistics...</p>
        </div>
      ) : error ? (
        <div className="flex items-center justify-center py-12">
          <p className="text-destructive">Error: {error}</p>
        </div>
      ) : stats ? (
        <Stats stats={stats} />
      ) : null}
    </div>
  )
}
