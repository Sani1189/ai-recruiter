import { apiClient } from "../client"

export interface CvFileData {
  id: string
  container: string
  filePath: string
  extension: string
  mbSize: number
  storageAccountName: string[]
  createdAt: string | null
  updatedAt: string | null
  createdBy: string | null
  updatedBy: string | null
}

export interface CvEvaluation {
  userProfileId: string
  promptCategory: string
  promptVersion: number
  fileId: string
  modelUsed: string
  responseJson: string
  id: string | null
  createdAt: string | null
  updatedAt: string | null
  createdBy: string | null
  updatedBy: string | null
}

class CvService {
  /**
   * Get CV evaluations for current user to extract the fileId
   * Changed primary method to fetch CV evaluations first to get fileId
   */
  async getUserCvEvaluations(token: string): Promise<CvEvaluation[]> {
    try {
      const data = await apiClient.get<CvEvaluation[]>(
        `/CvEvaluation/user-profile`,
        {
          requireAuth: true,
          cacheStrategy: "cache-first",
          cacheTTL: 3600000, // Cache for 1 hour
        },
        token,
      )
      console.log("] CV Evaluations fetched:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("] Failed to fetch CV evaluations:", error)
      return []
    }
  }

  /**
   * Get CV evaluations by userProfileId (admin endpoint for viewing candidate data)
   */
  async getCvEvaluationsByUserProfileId(token: string, userProfileId: string): Promise<CvEvaluation[]> {
    try {
      const data = await apiClient.get<CvEvaluation[]>(
        `/CvEvaluation/user-profile/${userProfileId}`,
        {
          requireAuth: true,
          cacheStrategy: "cache-first",
          cacheTTL: 3600000, // Cache for 1 hour
        },
        token,
      )
      console.log("] CV Evaluations fetched:", data)
      return Array.isArray(data) ? data : []
    } catch (error) {
      console.error("] Failed to fetch CV evaluations:", error)
      return []
    }
  }

  /**
   * Get CV file metadata using fileId
   * Changed to use fileId instead of userProfileId
   */
  async getCvFileData(token: string, fileId: string): Promise<CvFileData | null> {
    try {
      const response = await apiClient.get<CvFileData>(
        `/File/${fileId}`,
        {
          requireAuth: true,
          cacheStrategy: "cache-first",
          cacheTTL: 3600000, // Cache for 1 hour
        },
        token,
      )
      console.log("] CV File data fetched:", response.data)
      return response.data || null
    } catch (error) {
      console.error("] Failed to fetch CV file metadata:", error)
      return null
    }
  }

  /**
   * Get the latest CV file for current user
   * New convenience method that fetches evaluations first, then gets file data
   */
  async getUserLatestCvFile(token: string): Promise<CvFileData | null> {
    try {
      const evaluations = await this.getUserCvEvaluations(token)
      console.log("] Found evaluations:", evaluations.length)

      if (evaluations.length === 0) {
        console.log("] No CV evaluations found for user")
        return null
      }

      // Get the first evaluation (latest/most relevant)
      const latestEvaluation = evaluations[0]
      console.log("] Using fileId from evaluation:", latestEvaluation.fileId)

      return await this.getCvFileData(token, latestEvaluation.fileId)
    } catch (error) {
      console.error("] Failed to get latest CV file:", error)
      return null
    }
  }

  /**
   * Get download URL for CV file using fileId (returns SAS URL)
   */
  async getCvDownloadUrl(token: string, fileId: string, expirationMinutes: number = 5): Promise<string | null> {
    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5140/api"}/File/${fileId}/download?expirationMinutes=${expirationMinutes}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        },
      )

      if (!response.ok) {
        throw new Error(`Failed to get download URL: ${response.status}`)
      }

      const data = await response.json()
      return data.downloadUrl || null
    } catch (error) {
      console.error("] Failed to get CV download URL:", error)
      return null
    }
  }

  /**
   * Download CV file using fileId (legacy method - now uses SAS URL)
   * @deprecated Use getCvDownloadUrl and downloadFromSasUrl instead
   */
  async downloadCvFile(token: string, fileId: string): Promise<Blob | null> {
    try {
      const downloadUrl = await this.getCvDownloadUrl(token, fileId)
      if (!downloadUrl) {
        return null
      }

      const response = await fetch(downloadUrl)
      if (!response.ok) {
        throw new Error(`Download failed with status ${response.status}`)
      }

      return await response.blob()
    } catch (error) {
      console.error("] Failed to download CV file:", error)
      return null
    }
  }

  /**
   * Clear CV cache when needed (e.g., after upload)
   */
  clearCache() {
    apiClient.clearCache()
  }
}

export const cvService = new CvService()
