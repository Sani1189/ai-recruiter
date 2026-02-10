from azure.storage.blob import BlobServiceClient
from azure.core.exceptions import AzureError
import json
import uuid
from typing import Optional
import logging
from datetime import datetime
from config import settings
from core.file_helper import FilePathHelper

logger = logging.getLogger(__name__)

class AzureStorageService:
    def __init__(self):
        if not settings.azure_storage_connection_string or settings.azure_storage_connection_string.strip() == "":
            logger.error("AZURE_STORAGE_CONNECTION_STRING is empty or not set!")
            self.blob_service = None
            self.container_name = settings.azure_storage_container_name
            return
        
        try:
            self.blob_service = BlobServiceClient.from_connection_string(
                settings.azure_storage_connection_string
            )
            self.container_name = settings.azure_storage_container_name
            self._ensure_container_exists()
            logger.info("Azure Storage Service initialized successfully")
        except Exception as e:
            logger.error(f"Failed to initialize Azure Storage Service: {e}")
            self.blob_service = None
    
    def _ensure_container_exists(self):
        try:
            if not self.blob_service:
                logger.warning("Cannot create container - blob_service is None")
                return
            container_client = self.blob_service.get_container_client(self.container_name)
            container_client.create_container()
        except Exception as e:
            logger.info(f"Container {self.container_name} already exists or error: {e}")
    
    def upload_file(self, file_content: bytes, user_id: str, file_id: str, file_extension: str) -> tuple[str, str, bool]:
        """Upload file to blob storage. Returns (folder_path, file_name, success) matching C# pattern"""
        if not self.blob_service:
            logger.error("Azure Storage Service not initialized - connection string may be invalid")
            return "", "", False
        
        # Use helper for consistent path building
        folder_path, file_name = FilePathHelper.build_user_file_path(user_id, file_id, file_extension)
        full_blob_path = FilePathHelper.get_full_path(folder_path, file_name)
        
        try:
            blob_client = self.blob_service.get_blob_client(
                container=self.container_name,
                blob=full_blob_path
            )
            
            blob_client.upload_blob(file_content, overwrite=True)
            logger.info(f"File uploaded to blob storage: {full_blob_path}")
            return folder_path, file_name, True
        except AzureError as e:
            logger.error(f"Azure error uploading file to {full_blob_path}: {e}")
            return folder_path, file_name, False
        except Exception as e:
            logger.error(f"Unexpected error uploading file to {full_blob_path}: {e}")
            return folder_path, file_name, False
    
    def upload_json_response(self, json_data: dict, user_id: str, file_id: str) -> tuple[str, str, bool]:
        """Upload JSON response to blob storage. Returns (folder_path, file_name, success)"""
        if not self.blob_service:
            logger.error("Azure Storage Service not initialized")
            return "", "", False
        
        # Use helper for consistent path building
        folder_path, file_name = FilePathHelper.build_generated_file_path(user_id, file_id, "json")
        full_blob_path = FilePathHelper.get_full_path(folder_path, file_name)
        
        try:
            blob_client = self.blob_service.get_blob_client(
                container=self.container_name,
                blob=full_blob_path
            )
            
            json_content = json.dumps(json_data, indent=2)
            blob_client.upload_blob(json_content.encode('utf-8'), overwrite=True)
            logger.info(f"JSON response uploaded to blob storage: {full_blob_path}")
            return folder_path, file_name, True
        except AzureError as e:
            logger.error(f"Azure error uploading JSON to {full_blob_path}: {e}")
            return folder_path, file_name, False
        except Exception as e:
            logger.error(f"Unexpected error uploading JSON to {full_blob_path}: {e}")
            return folder_path, file_name, False
    
    def download_file(self, blob_path: str) -> Optional[bytes]:
        """Download file from blob storage"""
        if not self.blob_service:
            logger.error("Azure Storage Service not initialized")
            return None
        
        try:
            blob_client = self.blob_service.get_blob_client(
                container=self.container_name,
                blob=blob_path
            )
            return blob_client.download_blob().readall()
        except AzureError as e:
            logger.error(f"Azure error downloading file from {blob_path}: {e}")
            return None
        except Exception as e:
            logger.error(f"Unexpected error downloading file from {blob_path}: {e}")
            return None

azure_storage = AzureStorageService()
