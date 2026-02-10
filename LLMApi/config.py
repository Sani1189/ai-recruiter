import os
import re
from typing import List
from pydantic_settings import BaseSettings
from pydantic import AliasChoices, Field
from typing import Literal

class Settings(BaseSettings):
    # Database Configuration
    database_connection_string: str = os.getenv(
        "DATABASE_CONNECTION_STRING_PII", ""
    )
    
    sql_username: str = os.getenv("SQL_USERNAME", "")
    sql_password: str = os.getenv("SQL_PASSWORD", "")

    # Azure Storage - No default values for security
    azure_storage_connection_string: str = os.getenv("AZURE_STORAGE_CONNECTION_STRING", "")
    azure_storage_container_name: str = os.getenv("AZURE_STORAGE_CONTAINER_NAME", "cvfiles")

    # Azure Service Bus - No default values for security
    azure_service_bus_connection_string: str = os.getenv("AZURE_SERVICE_BUS_CONNECTION_STRING", "")
    azure_service_bus_queue_name: str = os.getenv("AZURE_SERVICE_BUS_QUEUE_NAME", "cv-processing-queue-test")

    # OpenAI - No default values for security
    openai_api_key: str = os.getenv("OPENAI_API_KEY", "")
    openai_model: str = os.getenv("OPENAI_MODEL", "gpt-4o")

    # Application Settings
    log_level: str = os.getenv("LOG_LEVEL", "INFO")
    max_file_size_mb: int = int(os.getenv("MAX_FILE_SIZE_MB", "10"))
    allowed_file_extensions: str = os.getenv("ALLOWED_FILE_EXTENSIONS", "pdf,doc,docx,txt")
    worker_poll_interval_seconds: int = int(os.getenv("WORKER_POLL_INTERVAL_SECONDS", "5"))
    max_retry_attempts: int = int(os.getenv("MAX_RETRY_ATTEMPTS", "3"))

    # Backend Base URL
    backend_base_url: str = os.getenv("BACKEND_BASE_URL", "http://localhost:5140")

    # Responses API uses `max_output_tokens`. We also accept `MAX_TOKENS` for backward compatibility.
    max_output_tokens: int = Field(default=4000, validation_alias=AliasChoices("MAX_OUTPUT_TOKENS", "MAX_TOKENS"))

    # GPT-5 family supports `text.verbosity` to steer output length.
    text_verbosity: Literal["medium"] = "medium"

    @property
    def allowed_extensions_list(self) -> List[str]:
        return [ext.strip() for ext in self.allowed_file_extensions.split(',')]
    
    @property
    def storage_account_name(self) -> str:
        """Extract storage account name from connection string"""
        if not self.azure_storage_connection_string:
            return "unknown"
        
        # Try to extract from AccountName=... pattern
        match = re.search(r'AccountName=([^;]+)', self.azure_storage_connection_string, re.IGNORECASE)
        if match:
            return match.group(1)
        
        # Try to extract from endpoint URL pattern
        match = re.search(r'https?://([^.]+)\.blob\.core\.windows\.net', self.azure_storage_connection_string, re.IGNORECASE)
        if match:
            return match.group(1)
        
        # Fallback for development
        return "devstoreaccount1" if "UseDevelopmentStorage=true" in self.azure_storage_connection_string else "unknown"

    def __post_init__(self):
        """Validate critical configuration on initialization"""
        missing_vars = []
        
        if not self.openai_api_key:
            missing_vars.append("OPENAI_API_KEY")
        if not self.azure_service_bus_connection_string:
            missing_vars.append("AZURE_SERVICE_BUS_CONNECTION_STRING")
        if not self.azure_storage_connection_string:
            missing_vars.append("AZURE_STORAGE_CONNECTION_STRING")
        if not self.database_connection_string:
            missing_vars.append("DATABASE_CONNECTION_STRING_PII")
        
        if missing_vars:
            raise ValueError(f"Missing required environment variables: {', '.join(missing_vars)}")

    class Config:
        env_file = ".env"

settings = Settings()
