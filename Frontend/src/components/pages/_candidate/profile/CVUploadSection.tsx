"use client"

import type React from "react"

import { useState } from "react"
import { toast } from "sonner"
import { Upload, FileText, CheckCircle, AlertCircle, Loader, X } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { cvService } from "@/lib/api/services/cv.service"
import type { CvFileData } from "@/lib/api/services/cv.service"
import { useApi } from "@/hooks/useApi"
import type { ApiResponse } from "@/lib/api"

export default function CVUploadSection({
  onRefresh,
  existingCvFile,
}: {
  onRefresh: () => void | Promise<void>
  existingCvFile: CvFileData | null
}) {
  const api = useApi()
  const [isUploading, setIsUploading] = useState(false)
  const [uploadStatus, setUploadStatus] = useState<"idle" | "success" | "error">("idle")
  const [uploadedFileName, setUploadedFileName] = useState<string>("")
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [showConfirmation, setShowConfirmation] = useState(false)

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    // Validate file type
    const validTypes = [
      "application/pdf",
      "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    ]
    if (!validTypes.includes(file.type)) {
      toast.error("Invalid file type", {
        description: "Please upload a PDF or Word document (.pdf, .doc, .docx)",
      })
      event.target.value = ""
      return
    }

    // Validate file size (max 10MB)
    const maxSize = 10 * 1024 * 1024
    if (file.size > maxSize) {
      toast.error("File too large", {
        description: "Please upload a file smaller than 10MB",
      })
      event.target.value = ""
      return
    }

    setSelectedFile(file)
    setShowConfirmation(true)
  }

  const handleCancelSelection = () => {
    setSelectedFile(null)
    setShowConfirmation(false)
  }

  const handleCVUpload = async (file: File) => {
    if (!file) {
      toast.error("Missing required information")
      return
    }

    setIsUploading(true)
    setUploadStatus("idle")

    try {
      // Create FormData for multipart/form-data upload
      const formData = new FormData()
      formData.append("file", file)
      formData.append("prompt_category", "cv_extraction")
      formData.append("prompt_version", "1")

      // Call C# API endpoint directly
      const result = await api.post<ApiResponse<any>>("/CvProcessing/upload", formData, {
        cacheStrategy: "no-cache",
        timeout: 60000, // 60 seconds timeout for file uploads
      })

      setUploadedFileName(file.name)
      setUploadStatus("success")
      setShowConfirmation(false)
      setSelectedFile(null)
      toast.success("CV uploaded successfully", {
        description: `Your CV "${file.name}" has been processed and uploaded.`,
      })

      // Clear cache and refresh after upload
      cvService.clearCache()

      // Reset after 2 seconds
      setTimeout(async () => {
        setUploadStatus("idle")
        setUploadedFileName("")
        await Promise.resolve(onRefresh())
      }, 2000)
    } catch (error) {
      setUploadStatus("error")
      toast.error("Upload failed", {
        description: error instanceof Error ? error.message : "Failed to upload CV. Please try again.",
      })
    } finally {
      setIsUploading(false)
    }
  }

  return (
    <Card className="border border-border shadow-xl bg-gradient-to-br from-card via-card/95 to-card overflow-hidden hover:shadow-2xl transition-shadow duration-300 dark:from-card dark:via-card dark:to-card">
      <CardHeader className="pb-8 border-b border-border bg-gradient-to-r from-card to-card/95 dark:from-card dark:to-card/80">
        <div className="space-y-2">
          <CardTitle className="text-3xl font-bold bg-gradient-to-r from-foreground to-muted-foreground bg-clip-text text-transparent flex items-center gap-3 dark:from-foreground dark:to-muted-foreground">
            <FileText className="h-8 w-8 text-primary dark:text-primary" />
            {existingCvFile ? "Your CV" : "Upload Your CV"}
          </CardTitle>
          <CardDescription className="text-base text-muted-foreground dark:text-muted-foreground">
            {existingCvFile
              ? "You have already uploaded a CV"
              : "Upload your CV to automatically extract and populate your profile information"}
          </CardDescription>
        </div>
      </CardHeader>

      <CardContent className="pt-8">
        <div className="space-y-6">
          {existingCvFile && (
            <div className="p-6 rounded-xl bg-emerald-50 dark:bg-emerald-950/30 border-2 border-emerald-200 dark:border-emerald-800 space-y-4">
              <div className="flex items-start gap-3">
                <CheckCircle className="h-6 w-6 text-emerald-600 dark:text-emerald-400 flex-shrink-0 mt-1" />
                <div className="flex-1">
                  <p className="font-semibold text-emerald-900 dark:text-emerald-200">CV Already Uploaded</p>
                  <p className="text-sm text-emerald-700 dark:text-emerald-300 mt-1">
                    You have already uploaded your CV. You can upload a new one if needed.
                  </p>
                </div>
              </div>
            </div>
          )}

          {!existingCvFile || uploadStatus === "success" ? (
            <div className="relative">
              <input
                type="file"
                id="cv-upload"
                onChange={handleFileSelect}
                disabled={isUploading || showConfirmation}
                accept=".pdf,.doc,.docx"
                className="hidden"
              />
              <label
                htmlFor="cv-upload"
                className={`flex flex-col items-center justify-center p-12 rounded-2xl border-2 border-dashed transition-all duration-300 ${
                  showConfirmation || isUploading
                    ? "border-border bg-muted dark:border-border dark:bg-muted cursor-not-allowed"
                    : "border-border bg-card hover:border-primary hover:bg-accent/50 dark:border-border dark:bg-card dark:hover:border-primary dark:hover:bg-accent/20 cursor-pointer"
                }`}
              >
                <div className="text-center space-y-4">
                  {isUploading ? (
                    <>
                      <Loader className="h-12 w-12 text-muted-foreground dark:text-muted-foreground mx-auto animate-spin" />
                      <p className="text-lg font-semibold text-foreground dark:text-foreground">Uploading your CV...</p>
                      <p className="text-sm text-muted-foreground dark:text-muted-foreground">
                        Please wait while we process your document
                      </p>
                    </>
                  ) : uploadStatus === "success" ? (
                    <>
                      <CheckCircle className="h-12 w-12 text-emerald-500 dark:text-emerald-400 mx-auto" />
                      <p className="text-lg font-semibold text-emerald-700 dark:text-emerald-300">Upload Successful!</p>
                      <p className="text-sm text-muted-foreground dark:text-muted-foreground">{uploadedFileName}</p>
                    </>
                  ) : uploadStatus === "error" ? (
                    <>
                      <AlertCircle className="h-12 w-12 text-red-500 dark:text-red-400 mx-auto" />
                      <p className="text-lg font-semibold text-red-700 dark:text-red-300">Upload Failed</p>
                      <p className="text-sm text-muted-foreground dark:text-muted-foreground">Please try again</p>
                    </>
                  ) : (
                    <>
                      <Upload className="h-12 w-12 text-muted-foreground dark:text-muted-foreground mx-auto" />
                      <div>
                        <p className="text-lg font-semibold text-foreground dark:text-foreground">
                          Drop your CV here or click to browse
                        </p>
                        <p className="text-sm text-muted-foreground dark:text-muted-foreground mt-2">
                          Supported formats: PDF, DOC, DOCX (Max 10MB)
                        </p>
                      </div>
                    </>
                  )}
                </div>
              </label>
            </div>
          ) : null}

          {showConfirmation && selectedFile && (
            <div className="p-6 rounded-xl bg-blue-50 dark:bg-blue-950/30 border-2 border-blue-200 dark:border-blue-800 space-y-4">
              <div className="flex items-start gap-3">
                <FileText className="h-6 w-6 text-blue-600 dark:text-blue-400 flex-shrink-0 mt-1" />
                <div className="flex-1">
                  <p className="font-semibold text-blue-900 dark:text-blue-200">Ready to upload?</p>
                  <p className="text-sm text-blue-700 dark:text-blue-300 mt-1">
                    File: <span className="font-mono font-semibold">{selectedFile.name}</span>
                  </p>
                  <p className="text-sm text-blue-700 dark:text-blue-300">
                    Size: {(selectedFile.size / 1024).toFixed(2)} KB
                  </p>
                </div>
              </div>
              <div className="flex gap-3 pt-2">
                <Button
                  onClick={() => handleCVUpload(selectedFile)}
                  disabled={isUploading}
                  className="flex-1 bg-emerald-600 hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-700 text-white font-semibold"
                >
                  {isUploading ? "Uploading..." : "Confirm Upload"}
                </Button>
                <Button
                  onClick={handleCancelSelection}
                  disabled={isUploading}
                  variant="outline"
                  className="flex-1 border-border text-foreground hover:bg-muted dark:border-border dark:text-foreground dark:hover:bg-muted bg-card"
                >
                  <X className="h-4 w-4 mr-2" />
                  Cancel
                </Button>
              </div>
            </div>
          )}

          <div className="p-5 rounded-xl bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-950/30 dark:to-blue-900/20 border border-blue-200 dark:border-blue-800">
            <p className="text-sm text-blue-900 dark:text-blue-200 leading-relaxed">
              <span className="font-semibold">Tip:</span> Uploading your CV will automatically extract your education,
              experience, skills, and other information to populate your profile.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {[
              { format: "PDF", icon: "📄", desc: "Portable Document Format" },
              { format: "DOC", icon: "📝", desc: "Microsoft Word Document" },
              { format: "DOCX", icon: "📋", desc: "Word Open XML Format" },
            ].map((item) => (
              <div
                key={item.format}
                className="p-4 rounded-lg bg-gradient-to-br from-card to-muted border border-border dark:from-card dark:to-muted text-center hover:shadow-md dark:hover:shadow-lg transition-all duration-200 dark:border-border"
              >
                <p className="text-2xl mb-2">{item.icon}</p>
                <p className="font-semibold text-foreground dark:text-foreground">{item.format}</p>
                <p className="text-xs text-muted-foreground dark:text-muted-foreground mt-1">{item.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
