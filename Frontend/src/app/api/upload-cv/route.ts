import { serverEnv } from "@/lib/config/env"
import { type NextRequest, NextResponse } from "next/server"

export async function POST(request: NextRequest) {
  try {
    const formData = await request.formData()

    // Get the file and other parameters from the request
    const file = formData.get("file") as File
    const promptCategory = formData.get("prompt_category") as string
    const promptVersion = formData.get("prompt_version") as string
    const userProfileId = formData.get("userProfileId") as string

    if (!file) {
      return NextResponse.json({ error: "No file provided" }, { status: 400 })
    }

    if (!userProfileId) {
      return NextResponse.json({ error: "User Profile ID is required" }, { status: 401 })
    }

    const azureFormData = new FormData()
    azureFormData.append("file", file)
    azureFormData.append("prompt_category", promptCategory || "cv_extraction")
    azureFormData.append("prompt_version", promptVersion || "1")
    azureFormData.append("userProfileId", userProfileId)

    const azureUrl =
      process.env.NODE_ENV === "development" ? "http://127.0.0.1:7071/api/upload-cv" : serverEnv.AzureFunctions.url

    if (!azureUrl) {
      return NextResponse.json({ error: "Azure Function URL not configured" }, { status: 500 })
    }

    console.log("Sending request to Azure:", azureUrl)
    console.log("File name:", file.name, "Size:", file.size, "Type:", file.type, "UserProfileId:", userProfileId)

    const controller = new AbortController()
    const timeoutId = setTimeout(() => controller.abort(), 30000) // 30 second timeout

    try {
      const response = await fetch(azureUrl, {
        method: "POST",
        body: azureFormData,
        signal: controller.signal,
      })

      clearTimeout(timeoutId)

      console.log("Azure response status:", response.status)

      if (!response.ok) {
        const errorData = await response.text()
        console.error("Azure backend error response:", errorData)
        return NextResponse.json(
          { error: `Azure backend error: ${response.statusText}`, details: errorData },
          { status: response.status },
        )
      }

      const result = await response.json()
      console.log("Azure upload successful:", result)
      return NextResponse.json(result)
    } catch (fetchError) {
      clearTimeout(timeoutId)
      console.error("Fetch error details:", fetchError instanceof Error ? fetchError.message : String(fetchError))
      throw fetchError
    }
  } catch (error) {
    console.error("CV upload API error:", error)
    return NextResponse.json(
      { error: error instanceof Error ? error.message : "Internal server error" },
      { status: 500 },
    )
  }
}
