"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { UseFormReturn } from "react-hook-form";
import { Image as ImageIcon, X, ExternalLink } from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useApi } from "@/hooks/useApi";
import { uploadToSasUrl } from "@/lib/fileUtils";
import { toast } from "sonner";
import { AssessmentTemplateFormData } from "@/schemas/assessmentTemplate.schema";

type UploadUrlResponse = {
  uploadUrl: string;
  container: string;
  folderPath?: string | null;
  filePath: string;
  storageAccountName: string;
  blobUrl: string;
  expiresInMinutes: number;
};

type CreatedFileResponse = {
  id: string;
  container: string;
  folderPath?: string | null;
  filePath: string;
  extension: string;
  mbSize: number;
  storageAccountName: string;
};

export function MediaUploadField({
  label,
  accept = "image/*",
  compact = false,
  iconOnly = false,
  form,
  urlPath,
  fileIdPath,
}: {
  label: string;
  accept?: string;
  compact?: boolean;
  iconOnly?: boolean;
  form: UseFormReturn<AssessmentTemplateFormData, any, AssessmentTemplateFormData>;
  urlPath: any;
  fileIdPath: any;
}) {
  const api = useApi();
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [uploading, setUploading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const fileId = (form.watch(fileIdPath) as string | null | undefined) ?? null;
  const blobUrl = (form.watch(urlPath) as string | null | undefined) ?? null;

  const canPreview = useMemo(() => {
    return !!(previewUrl || blobUrl);
  }, [previewUrl, blobUrl]);

  useEffect(() => {
    const loadPreview = async () => {
      if (!fileId) {
        setPreviewUrl(null);
        return;
      }
      try {
        const res = await api.get(`/File/${fileId}/download-url`);
        const data = (res as any)?.data ?? res;
        if (data?.downloadUrl) setPreviewUrl(data.downloadUrl);
      } catch {
        // If we can't get SAS (policy mismatch), fall back to blobUrl (may be public in dev).
        setPreviewUrl(null);
      }
    };
    loadPreview();
  }, [fileId]);

  const clear = () => {
    form.setValue(urlPath, null);
    form.setValue(fileIdPath, null);
    setPreviewUrl(null);
  };

  const onPickFile = async (file: File) => {
    if (!file) return;
    setUploading(true);
    try {
      // 1) get upload SAS + target blob url
      const uploadRes = await api.post<UploadUrlResponse>("/File/upload/get-upload-url", {
        fileName: file.name,
        contentType: file.type || "application/octet-stream",
        fileSize: file.size,
        folderPrefix: "assessment-templates",
      });
      const uploadData: UploadUrlResponse = (uploadRes as any)?.data ?? uploadRes;
      if (!uploadData?.uploadUrl || !uploadData?.filePath || !uploadData?.container) {
        throw new Error("Failed to get upload URL from server");
      }

      // 2) upload directly to Azure
      await uploadToSasUrl(file, uploadData.uploadUrl);

      // 3) create File record (so we can generate SAS download URLs securely later)
      const ext = (() => {
        const dot = file.name.lastIndexOf(".");
        return dot >= 0 ? file.name.substring(dot).toLowerCase() : "";
      })();
      const mbSize = Math.max(1, Math.ceil(file.size / (1024 * 1024)));

      const created = await api.post<CreatedFileResponse>("/File", {
        container: uploadData.container,
        folderPath: uploadData.folderPath ?? null,
        filePath: uploadData.filePath,
        extension: ext || ".bin",
        mbSize,
        storageAccountName: uploadData.storageAccountName,
      });
      const createdData: CreatedFileResponse = (created as any)?.data ?? created;
      if (!createdData?.id) {
        throw new Error("Upload succeeded but file metadata could not be saved");
      }

      form.setValue(urlPath, uploadData.blobUrl);
      form.setValue(fileIdPath, createdData.id);
      toast.success("Media uploaded");
    } catch (e: any) {
      toast.error(e?.message || "Failed to upload media");
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="space-y-2">
      {!compact && (
        <div className="flex items-center justify-between">
          <Label>{label}</Label>
          {(blobUrl || fileId) && (
            <Button type="button" variant="ghost" size="sm" onClick={clear} disabled={uploading}>
              <X className="h-4 w-4 mr-2" />
              Remove
            </Button>
          )}
        </div>
      )}

      {compact ? (
        <div className="flex items-center gap-2">
          <input
            ref={inputRef}
            type="file"
            accept={accept}
            disabled={uploading}
            className="hidden"
            onChange={(e) => {
              const f = e.target.files?.[0];
              if (f) void onPickFile(f);
              e.currentTarget.value = "";
            }}
          />
          {iconOnly ? (
            <div className="relative h-9 w-9 shrink-0">
              <Button
                type="button"
                variant="outline"
                size="icon"
                disabled={uploading}
                className="h-9 w-9 p-0 overflow-hidden"
                onClick={() => inputRef.current?.click()}
                title={blobUrl || fileId ? `Change ${label}` : `Add ${label}`}
                aria-label={blobUrl || fileId ? `Change ${label}` : `Add ${label}`}
              >
                {canPreview ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={previewUrl || blobUrl || ""}
                    alt="uploaded media"
                    className="h-9 w-9 object-cover"
                  />
                ) : (
                  <ImageIcon className="h-4 w-4" />
                )}
              </Button>

              {canPreview && (
                <Button
                  type="button"
                  variant="secondary"
                  size="icon"
                  className="absolute -top-2 -right-2 h-6 w-6 rounded-full shadow"
                  onClick={clear}
                  disabled={uploading}
                  title="Remove image"
                >
                  <X className="h-3 w-3" />
                </Button>
              )}
            </div>
          ) : (
            <>
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={uploading}
                onClick={() => inputRef.current?.click()}
              >
                <ImageIcon className="h-4 w-4 mr-2" />
                {blobUrl || fileId ? "Change" : "Add image"}
              </Button>

              {canPreview && (
                <div className="relative h-9 w-9">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src={previewUrl || blobUrl || ""}
                    alt="uploaded media"
                    className="h-9 w-9 rounded-md object-cover border"
                  />
                  <Button
                    type="button"
                    variant="secondary"
                    size="icon"
                    className="absolute -top-2 -right-2 h-6 w-6 rounded-full shadow"
                    onClick={clear}
                    disabled={uploading}
                    title="Remove image"
                  >
                    <X className="h-3 w-3" />
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      ) : (
        <div className="flex gap-2">
          <Input
            ref={inputRef as any}
            type="file"
            accept={accept}
            disabled={uploading}
            onChange={(e) => {
              const f = e.target.files?.[0];
              if (f) void onPickFile(f);
              e.currentTarget.value = "";
            }}
          />
        </div>
      )}

      {uploading && (
        <div className="text-xs text-muted-foreground">Uploading to Azure Blob…</div>
      )}

      {!compact && canPreview && (
        <div className="mt-2 border rounded-md p-2">
          <div className="flex items-center justify-between mb-2">
            <div className="text-xs text-muted-foreground">Preview</div>
            {blobUrl && (
              <Link
                href={blobUrl}
                target="_blank"
                className="text-xs text-muted-foreground hover:underline inline-flex items-center gap-1"
              >
                Open <ExternalLink className="h-3 w-3" />
              </Link>
            )}
          </div>
          {/* If blob is private, previewUrl will be SAS; otherwise fallback to blobUrl */}
          <img
            src={previewUrl || blobUrl || ""}
            alt="uploaded media"
            className="max-h-64 w-auto rounded-md"
          />
        </div>
      )}

      {!compact && (
        <div className="text-xs text-muted-foreground">
          Uploads to Azure Blob via SAS URL (same pattern as resume upload). We save a stable blob URL plus a File id for secure access.
        </div>
      )}
    </div>
  );
}


