"use client"

import { useState } from "react"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog"
import { Button } from "@/components/ui/button"
import { toast } from "sonner"

interface ConfirmDialogProps<T = any> {
  trigger?: React.ReactNode
  title?: string
  description?: string
  confirmText?: string
  cancelText?: string
  payload?: T
  onConfirm: (payload?: T) => Promise<void> | void
  onCancel?: () => void
  variant?: "default" | "destructive" | "outline" | "secondary"
  disabled?: boolean
  showToast?: boolean
  successMessage?: string
  errorMessage?: string
}

export function ConfirmDialog<T>({
  trigger,
  title = "Are you absolutely sure?",
  description = "This action cannot be undone.",
  confirmText = "Confirm",
  cancelText = "Cancel",
  payload,
  onConfirm,
  onCancel,
  variant = "destructive",
  disabled = false,
  showToast = true,
  successMessage = "Action completed successfully",
  errorMessage = "Failed to complete action",
}: ConfirmDialogProps<T>) {
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)

  const handleConfirm = async (e: React.MouseEvent) => {
    e.preventDefault()
    try {
      setLoading(true)
      await onConfirm(payload)
      setOpen(false)
      if (showToast) {
        toast.success(successMessage)
      }
    } catch (err) {
      console.error(err)
      if (showToast) {
        toast.error(errorMessage)
      }
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    onCancel?.()
    setOpen(false)
  }

  return (
    <AlertDialog open={open} onOpenChange={setOpen}>
      <AlertDialogTrigger asChild disabled={disabled}>
        {trigger ?? (
          <Button variant={variant} disabled={disabled}>
            Delete
          </Button>
        )}
      </AlertDialogTrigger>

      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>

        <AlertDialogFooter>
          <AlertDialogCancel disabled={loading} onClick={handleCancel}>
            {cancelText}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={loading}
            className={variant === "destructive" ? "bg-red-600 hover:bg-red-700" : ""}
          >
            {loading ? "Processing..." : confirmText}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
