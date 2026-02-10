"""
File path helper utilities matching C# BlobFilePathHelper pattern
Provides consistent file path handling across the application
"""
from typing import Tuple


class FilePathHelper:
    """Helper class for managing blob file paths consistently with C# backend"""
    
    @staticmethod
    def parse_blob_path(full_blob_path: str) -> Tuple[str | None, str]:
        """
        Parse a full blob path into folder path and file name
        
        Args:
            full_blob_path: Full path like "UserFiles/userId/file.pdf"
            
        Returns:
            Tuple of (folder_path, file_name) or (None, file_name) if no folder
        """
        if not full_blob_path:
            return None, ""
        
        if '/' in full_blob_path:
            parts = full_blob_path.rsplit('/', 1)
            return parts[0], parts[1]
        
        return None, full_blob_path
    
    @staticmethod
    def build_user_file_path(user_id: str, file_id: str, extension: str) -> Tuple[str, str]:
        """
        Build file path for user uploaded files
        
        Args:
            user_id: User profile ID
            file_id: File ID (usually UUID)
            extension: File extension without dot
            
        Returns:
            Tuple of (folder_path, file_name)
        """
        folder_path = f"UserFiles/{user_id}"
        file_name = f"{file_id}.{extension}"
        return folder_path, file_name
    
    @staticmethod
    def build_generated_file_path(user_id: str, file_id: str, extension: str = "json") -> Tuple[str, str]:
        """
        Build file path for generated/processed files
        
        Args:
            user_id: User profile ID
            file_id: File ID (usually UUID)
            extension: File extension without dot (default: json)
            
        Returns:
            Tuple of (folder_path, file_name)
        """
        folder_path = f"UserFiles/{user_id}/Generated"
        file_name = f"{file_id}.{extension}"
        return folder_path, file_name
    
    @staticmethod
    def get_full_path(folder_path: str | None, file_name: str) -> str:
        """
        Combine folder path and file name into full blob path
        
        Args:
            folder_path: Folder path or None
            file_name: File name
            
        Returns:
            Full blob path
        """
        if folder_path:
            return f"{folder_path}/{file_name}"
        return file_name
    
    @staticmethod
    def sanitize_file_name(filename: str) -> str:
        """
        Sanitize file name for safe storage
        
        Args:
            filename: Original filename
            
        Returns:
            Sanitized filename
        """
        # Remove or replace unsafe characters
        unsafe_chars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|']
        sanitized = filename
        for char in unsafe_chars:
            sanitized = sanitized.replace(char, '_')
        return sanitized.strip()
