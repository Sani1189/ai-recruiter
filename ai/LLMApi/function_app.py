import azure.functions as func
import logging
import json
import os
import sys
import uuid
import asyncio
from azure_functions.cv_processor import cv_processor
from database import SessionLocal, set_current_user

from config import settings
from models.database import File as FileModel, UserProfile, Candidate
from models.pydantic_models import TranscriptFileReference, TranscriptItem
from services.interview_scoring_service import interview_scoring_service

logger = logging.getLogger(__name__)

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from services.azure_storage import azure_storage
from services.service_bus import service_bus

app = func.FunctionApp()

@app.route(route="swagger", methods=["GET"])
def swagger_ui(req: func.HttpRequest) -> func.HttpResponse:
    """Swagger UI for API testing"""
    swagger_html = """
    <!DOCTYPE html>
    <html>
    <head>
        <title>CV Processing API - Swagger UI</title>
        <link rel="stylesheet" type="text/css" href="https://unpkg.com/swagger-ui-dist@3.25.0/swagger-ui.css" />
        <style>
            html { box-sizing: border-box; overflow: -moz-scrollbars-vertical; overflow-y: scroll; }
            *, *:before, *:after { box-sizing: inherit; }
            body { margin:0; background: #fafafa; }
        </style>
    </head>
    <body>
        <div id="swagger-ui"></div>
        <script src="https://unpkg.com/swagger-ui-dist@3.25.0/swagger-ui-bundle.js"></script>
        <script src="https://unpkg.com/swagger-ui-dist@3.25.0/swagger-ui-standalone-preset.js"></script>
        <script>
            window.onload = function() {
                const ui = SwaggerUIBundle({
                    url: '/api/openapi.json',
                    dom_id: '#swagger-ui',
                    deepLinking: true,
                    presets: [
                        SwaggerUIBundle.presets.apis,
                        SwaggerUIStandalonePreset
                    ],
                    plugins: [
                        SwaggerUIBundle.plugins.DownloadUrl
                    ],
                    layout: "StandaloneLayout"
                });
            };
        </script>
    </body>
    </html>
    """
    return func.HttpResponse(swagger_html, mimetype="text/html")

@app.route(route="openapi.json", methods=["GET"])
def openapi_spec(req: func.HttpRequest) -> func.HttpResponse:
    """OpenAPI specification for the CV Processing API"""
    
    openapi_spec = {
        "openapi": "3.0.0",
        "info": {
            "title": "CV Processing API",
            "description": "API for uploading and processing CV files using Azure Functions",
            "version": "1.0.0"
        },
        "servers": [
            {"url": "/api", "description": "Azure Functions API"}
        ],
        "paths": {
            "/": {
                "get": {
                    "summary": "Root endpoint",
                    "responses": {
                        "200": {
                            "description": "Success",
                            "content": {
                                "application/json": {
                                    "schema": {
                                        "type": "object",
                                        "properties": {
                                            "message": {"type": "string"}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            "/health": {
                "get": {
                    "summary": "Health check",
                    "responses": {
                        "200": {
                            "description": "API is healthy",
                            "content": {
                                "application/json": {
                                    "schema": {
                                        "type": "object",
                                        "properties": {
                                            "status": {"type": "string"},
                                            "version": {"type": "string"}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            "/upload-cv": {
                "post": {
                    "summary": "Upload CV file",
                    "requestBody": {
                        "content": {
                            "multipart/form-data": {
                                "schema": {
                                    "type": "object",
                                    "properties": {
                                        "file": {
                                            "type": "string",
                                            "format": "binary"
                                        },
                                        "prompt_category": {
                                            "type": "string",
                                            "default": "cv_extraction"
                                        },
                                        "prompt_version": {
                                            "type": "integer",
                                            "default": 1
                                        }
                                    },
                                    "required": ["file"]
                                }
                            }
                        }
                    },
                    "responses": {
                        "200": {
                            "description": "File uploaded successfully"
                        }
                    }
                }
            },
            "/cv-status/{file_id}": {
                "get": {
                    "summary": "Get CV processing status",
                    "parameters": [
                        {
                            "name": "file_id",
                            "in": "path",
                            "required": True,
                            "schema": {
                                "type": "string"
                            }
                        }
                    ],
                    "responses": {
                        "200": {
                            "description": "Success",
                            "content": {
                                "application/json": {
                                    "schema": {
                                        "type": "object",
                                        "properties": {
                                            "status": {"type": "string"},
                                            "file_id": {"type": "string"},
                                            "message": {"type": "string"}
                                        }
                                    }
                                }
                            }
                        },
                        "400": {
                            "description": "Bad request",
                            "content": {
                                "application/json": {
                                    "schema": {
                                        "type": "object",
                                        "properties": {
                                            "error": {"type": "string"}
                                        }
                                    }
                                }
                            }
                        },
                        "500": {
                            "description": "Internal server error",
                            "content": {
                                "application/json": {
                                    "schema": {
                                        "type": "object",
                                        "properties": {
                                            "error": {"type": "string"}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    return func.HttpResponse(
        json.dumps(openapi_spec, indent=2),
        mimetype="application/json"
    )

@app.route(route="health", methods=["GET"], auth_level=func.AuthLevel.ANONYMOUS)
def health_check_http(req: func.HttpRequest) -> func.HttpResponse:
    """Health check endpoint"""
    return func.HttpResponse(
        json.dumps({"status": "healthy", "version": "1.0.0"}),
        status_code=200,
        mimetype="application/json"
    )

@app.route(route="", methods=["GET"])
def root_http(req: func.HttpRequest) -> func.HttpResponse:
    """Root endpoint"""
    return func.HttpResponse(
        json.dumps({"message": "CV Processing API is running on Azure Functions"}),
        status_code=200,
        mimetype="application/json"
    )

@app.route(route="upload-cv", methods=["POST"])
async def upload_file(req: func.HttpRequest) -> func.HttpResponse:
    """HTTP-triggered Azure Function to upload CV and queue for processing"""
    try:
        # Parse multipart form-data
        form = req.form
        uploaded_file = req.files.get('file')
        # <CHANGE> Get userProfileId from form data (the real UserProfile ID)
        user_id = form.get('userProfileId')
        
        if not uploaded_file:
            return func.HttpResponse(
                json.dumps({"error": "No file provided"}),
                status_code=400,
                mimetype="application/json"
            )

        # <CHANGE> Validate userProfileId is provided
        if not user_id:
            return func.HttpResponse(
                json.dumps({"error": "User Profile ID is required"}),
                status_code=401,
                mimetype="application/json"
            )

        file_content = uploaded_file.read()
        filename = uploaded_file.filename
        prompt_name = form.get('prompt_name') or form.get('promptName')
        prompt_category = form.get('prompt_category', 'cv_extraction')
        prompt_version = int(form.get('prompt_version', 1))

        # Validate file extension
        file_extension = filename.split('.')[-1].lower()
        if file_extension not in settings.allowed_extensions_list:
            return func.HttpResponse(
                json.dumps({
                    "error": f"File type not allowed. Allowed types: {settings.allowed_file_extensions}"
                }),
                status_code=400,
                mimetype="application/json"
            )

        # Validate file size
        file_size_mb = len(file_content) / (1024 * 1024)
        if file_size_mb > settings.max_file_size_mb:
            return func.HttpResponse(
                json.dumps({
                    "error": f"File too large. Maximum size: {settings.max_file_size_mb}MB"
                }),
                status_code=400,
                mimetype="application/json"
            )

        db = SessionLocal()
        
        # Get user email for audit context
        user_profile = db.query(UserProfile).filter(
            UserProfile.Id == user_id,
            UserProfile.IsDeleted == False
        ).first()
        
        if not user_profile:
            db.close()
            return func.HttpResponse(
                json.dumps({"error": "User not found. User must be created before uploading files."}),
                status_code=404,
                mimetype="application/json"
            )
        
        # Set audit context with user email
        set_current_user(user_profile.Email or user_id)

        # Generate file ID
        file_id = str(uuid.uuid4())

        # Upload to blob storage using C# pattern
        folder_path, file_name, upload_success = azure_storage.upload_file(
            file_content, user_id, file_id, file_extension
        )
        
        if not upload_success:
            logger.error(f"Failed to upload file to Azure Blob Storage for file_id: {file_id}")
            db.close()
            return func.HttpResponse(
                json.dumps({
                    "error": "Failed to upload file to storage.",
                }),
                status_code=500,
                mimetype="application/json"
            )
        
        # Save file record matching C# pattern (FilePath = filename only)
        file_record = FileModel(
            Id=file_id,
            Container=settings.azure_storage_container_name,
            FolderPath=folder_path,
            FilePath=file_name,  # Only filename, not full path
            Extension=file_extension,
            MbSize=int(file_size_mb),
            StorageAccountName=settings.storage_account_name
        )
        db.add(file_record)

        # <CHANGE> Check if Candidate exists, update or create
        candidate = db.query(Candidate).filter(
            Candidate.UserId == user_id,
            Candidate.IsDeleted == False
        ).first()

        if not candidate:
            candidate = Candidate(
                Id=str(uuid.uuid4()),
                UserId=user_id,
                CandidateId="",  # Will be set by C# backend if needed
                CvFileId=file_id
            )
            db.add(candidate)
            logger.info(f"Created new Candidate for user: {user_id}")
        else:
            # Update existing candidate with new CV file
            candidate.CvFileId = file_id
            logger.info(f"Updated Candidate CV for user: {user_id}")

        # Update UserProfile.ResumeUrl with full blob path (matching C# pattern)
        full_blob_path = f"{folder_path}/{file_name}"
        user_profile = db.query(UserProfile).filter(
            UserProfile.Id == user_id,
            UserProfile.IsDeleted == False
        ).first()
        
        if user_profile:
            user_profile.ResumeUrl = full_blob_path
            logger.info(f"Updated UserProfile.ResumeUrl to: {full_blob_path}")

        db.commit()
        logger.info(f"Created/Updated File, Candidate, and UserProfile: {file_id}")

        # Prepare message with full blob path for processing
        full_blob_path = f"{folder_path}/{file_name}"
        message_data = {
            "userId": user_id,
            "fileId": file_id,
            "fileExtension": file_extension,
            "promptName": prompt_name,
            "promptCategory": prompt_category,
            "promptVersion": prompt_version,
            "blobPath": full_blob_path,
            "folderPath": folder_path,
            "fileName": file_name
        }

        queue_success = await service_bus.send_cv_processing_message(message_data)

        # Response
        if queue_success:
            logger.info(f"CV uploaded and queued: {file_id} for user: {user_id}")
            db.close()
            return func.HttpResponse(
                json.dumps({
                    "fileId": file_id,
                    "userProfileId": user_id,
                    "message": "CV uploaded and queued for processing",
                    "status": "queued"
                }),
                status_code=200,
                mimetype="application/json"
            )
        else:
            logger.error(f"Failed to queue CV: {file_id}")
            db.close()
            return func.HttpResponse(
                json.dumps({
                    "fileId": file_id,
                    "userProfileId": user_id,
                    "message": "CV uploaded but failed to queue for processing",
                    "status": "upload_failed"
                }),
                status_code=500,
                mimetype="application/json"
            )

    except Exception as e:
        logger.error(f"Error uploading CV: {e}")
        return func.HttpResponse(
            json.dumps({"error": str(e)}),
            status_code=500,
            mimetype="application/json"
        )

@app.route(route="cv-status/{file_id}", methods=["GET"])
def get_cv_status_http(req: func.HttpRequest) -> func.HttpResponse:
    """Get CV processing status - simplified version"""
    logging.info('CV status request received')
    
    try:
        file_id = req.route_params.get('file_id')
        
        if not file_id:
            return func.HttpResponse(
                json.dumps({"error": "File ID is required"}),
                status_code=400,
                mimetype="application/json"
            )
        
        # Simple test response
        response_data = {
            "status": "completed",
            "file_id": file_id,
            "message": "CV processing completed (test mode)"
        }
        
        return func.HttpResponse(
            json.dumps(response_data),
            status_code=200,
            mimetype="application/json"
        )
        
    except Exception as e:
        logging.error(f"Error getting CV status: {e}")
        return func.HttpResponse(
            json.dumps({"error": str(e)}),
            status_code=500,
            mimetype="application/json"
        )

@app.route(route="webhook", methods=["POST"])
async def webhook_elevenlabs_transcript(req: func.HttpRequest) -> func.HttpResponse:
    """Handle ElevenLabs transcript webhook"""
    try:
        # No signature validation here.
        # signature validation is done in the C# backend before forwarding.
        payload = req.get_body()
        transcriptFileRef = TranscriptFileReference(**json.loads(payload))

        # Download transcript from blob storage
        blob_content = azure_storage.download_file(transcriptFileRef.blob_path)
        if not blob_content:
            raise Exception("Failed to download transcript file from blob storage")
        
        # Parse transcript JSON
        transcript_data = json.loads(blob_content.decode('utf-8'))
        transcript = [TranscriptItem(**item) for item in transcript_data]

        # Process transcript for interview scoring
        success, message = await interview_scoring_service.process_transcript(transcript, transcriptFileRef.conversation_id)

        if not success:
            raise Exception(message)

        return func.HttpResponse(
            json.dumps({"status": "received"}),
            status_code=200,
            mimetype="application/json"
        )
    except Exception as e:
        logging.error(f"Error in ElevenLabs webhook: {e}")
        return func.HttpResponse(
            json.dumps({"error": str(e)}),
            status_code=500,
            mimetype="application/json"
        )

# These can be re-enabled once basic functionality is working

# @app.route(route="user-cvs/{user_id}", methods=["GET"])
# def get_user_cvs_http(req: func.HttpRequest) -> func.HttpResponse:
#     """Get all CV evaluations for a user"""
#     # Implementation here...

# @app.route(route="user-profile/{user_id}", methods=["GET"])  
# def get_user_profile_http(req: func.HttpRequest) -> func.HttpResponse:
#     """Get complete user profile"""
#     # Implementation here...

# Service Bus triggers commented out for now
@app.service_bus_queue_trigger(
    arg_name="msg", 
    queue_name="cv-processing-queue-test",
    connection="AZURE_SERVICE_BUS_CONNECTION_STRING"
)
def cv_processor_function(msg: func.ServiceBusMessage) -> None:
    """Azure Function to process CV messages from Service Bus queue"""
    
    logging.info("CV processing started")
    
    try:
        # Get message content
        message_body = msg.get_body().decode('utf-8')
        message_data = json.loads(message_body)
        
        delivery_count = msg.delivery_count
        logging.info(f"Processing CV for user: {message_data.get('userId')}, delivery attempt: {delivery_count}")
        
        if 'fileContent' in message_data and isinstance(message_data['fileContent'], str):
            try:
                message_data['fileContent'] = bytes.fromhex(message_data['fileContent'])
            except ValueError:
                message_data['fileContent'] = message_data['fileContent'].encode('utf-8')
        
        db = SessionLocal()
        
        try:
            loop = asyncio.get_event_loop()
        except RuntimeError:
            loop = asyncio.new_event_loop()
            asyncio.set_event_loop(loop)
        
        loop.run_until_complete(cv_processor.process_cv_direct(message_data, db))
        
        logging.info("CV processed successfully")
        
    except Exception as e:
        error_msg = str(e)
        logging.error(f"Error processing CV: {error_msg}")
        
        # Don't retry for certain permanent errors
        permanent_errors = [
            "File not found",
            "Invalid file format", 
            "User not found",
            "Invalid message format",
            "Missing required fields"
        ]
        
        is_permanent_error = any(perm_error in error_msg for perm_error in permanent_errors)
        
        if is_permanent_error:
            logging.error(f"Permanent error detected, not retrying: {error_msg}")
            return
        
        # For transient errors, raise to trigger retry
        raise

@app.service_bus_queue_trigger(
    arg_name="dlq_msg",
    queue_name="cv-processing-queue-test/$deadletterqueue", 
    connection="AZURE_SERVICE_BUS_CONNECTION_STRING"
)
def dead_letter_processor_function(dlq_msg: func.ServiceBusMessage) -> None:
    """Process messages from the dead letter queue"""
    
    logging.info("Processing dead letter message")
    
    try:
        message_body = dlq_msg.get_body().decode('utf-8')
        message_data = json.loads(message_body)
        
        user_id = message_data.get('userId')
        file_id = message_data.get('fileId')
        
        logging.error(f"Dead letter message for user {user_id}, file {file_id}")
        logging.error(f"Dead letter reason: {dlq_msg.dead_letter_reason}")
        logging.error(f"Dead letter description: {dlq_msg.dead_letter_error_description}")
        
        # TODO: Implement notification system to alert administrators
        # TODO: Store failed processing attempts in database for analysis
        # TODO: Implement manual retry mechanism for dead letter messages
        
    except Exception as e:
        logging.error(f"Error processing dead letter message: {str(e)}")
