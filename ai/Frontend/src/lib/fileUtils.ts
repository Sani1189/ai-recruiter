/**
 * Utility functions for file operations
 */

/**
 * Downloads a blob as a file with the specified filename
 * @param blob - The blob to download
 * @param fileName - The name of the file to download
 */
export function downloadBlob(blob: Blob, fileName: string): void {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
}

/**
 * Downloads a file from a SAS URL (Azure Blob Storage)
 * @param sasUrl - The SAS URL from the backend API
 * @param fileName - The name of the file to download
 */
export async function downloadFromSasUrl(sasUrl: string, fileName: string): Promise<void> {
  try {
    const response = await fetch(sasUrl);
    if (!response.ok) {
      throw new Error(`Failed to download file: ${response.status} ${response.statusText}`);
    }
    const blob = await response.blob();
    downloadBlob(blob, fileName);
  } catch (error) {
    console.error('Error downloading from SAS URL:', error);
    throw error;
  }
}

/**
 * Uploads a file directly to Azure Blob Storage using a SAS URL
 * @param file - The file to upload
 * @param uploadUrl - The SAS URL from the backend API
 * @param onProgress - Optional progress callback (0-100)
 * @returns Promise that resolves when upload is complete
 */
export async function uploadToSasUrl(
  file: File,
  uploadUrl: string,
  onProgress?: (progress: number) => void
): Promise<void> {
  try {
    // Use fetch API for better CORS handling
    const response = await fetch(uploadUrl, {
      method: 'PUT',
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type || 'application/octet-stream',
      },
      body: file,
    });

    if (!response.ok) {
      const errorText = await response.text().catch(() => response.statusText);
      throw new Error(`Upload failed with status ${response.status}: ${errorText}`);
    }
  } catch (error) {
    if (error instanceof TypeError && error.message.includes('CORS') || error instanceof TypeError && error.message.includes('Failed to fetch')) {
      throw new Error(
        'CORS error: Azure Storage Account CORS configuration may be incorrect. ' +
        'Please ensure CORS is configured for your frontend origin with methods: GET, PUT, HEAD, POST, DELETE, OPTIONS'
      );
    }
    throw error;
  }
}

