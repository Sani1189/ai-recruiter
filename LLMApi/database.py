import logging
import urllib.parse
import struct
import os
from contextvars import ContextVar
from sqlalchemy import create_engine, event
from sqlalchemy.orm import sessionmaker, Session
from azure.identity import DefaultAzureCredential
from config import settings

logger = logging.getLogger(__name__)

# Context variable to track current user for audit fields
_current_user: ContextVar[str] = ContextVar('current_user', default='system')

engine = None
SessionLocal = None

def set_current_user(user_id: str) -> None:
    """Set the current user for audit tracking"""
    _current_user.set(user_id)

def get_current_user() -> str:
    """Get the current user for audit tracking"""
    return _current_user.get()

def audit_context(user_id: str):
    """Context manager for setting audit user"""
    class AuditContext:
        def __enter__(self):
            set_current_user(user_id)
            return self
        
        def __exit__(self, *args):
            set_current_user('system')
    
    return AuditContext()

def create_sqlalchemy_url(connection_string: str) -> str:
    """Convert ODBC connection string to SQLAlchemy URL"""
    try:
        # Quote the ODBC string for SQLAlchemy
        return f"mssql+pyodbc:///?odbc_connect={urllib.parse.quote_plus(connection_string)}"
    except Exception as e:
        logger.error(f"Error creating SQLAlchemy URL: {e}")
        raise

def should_use_managed_identity() -> bool:
    """Determine if we should use Managed Identity based on environment"""
    conn_str = settings.database_connection_string.lower()
    
    # Check if connection string has SQL authentication credentials
    has_sql_auth = any(auth in conn_str for auth in ['uid=', 'user id=', 'pwd=', 'password='])
    
    # Check if using Windows Authentication (Trusted Connection) - should NOT use Managed Identity
    has_trusted_connection = any(auth in conn_str for auth in ['trusted_connection=', 'integrated security='])
    
    # Only use Managed Identity if no SQL auth AND no Windows auth (i.e., Azure production environment)
    use_managed_identity = not has_sql_auth and not has_trusted_connection
    
    logger.info(f"Authentication check - Has SQL Auth: {has_sql_auth}, Has Trusted Connection: {has_trusted_connection}, Using Managed Identity: {use_managed_identity}")
    return use_managed_identity

def get_connection_string_with_auth() -> str:
    """Get connection string with appropriate authentication method"""
    base_conn_str = settings.database_connection_string
    
    if should_use_managed_identity():
        # When using access token, connection string must be clean of auth parameters
        logger.info("Using Managed Identity authentication with access token")
        return base_conn_str.replace(';Authentication=ActiveDirectoryMsi', '')
    else:
        logger.info("Using SQL Server authentication from connection string")
        return base_conn_str

def initialize_database():
    """Initialize SQLAlchemy engine with appropriate authentication"""
    global engine, SessionLocal

    try:
        connection_string = get_connection_string_with_auth()
        safe_conn_str = connection_string.replace('AccountKey=', 'AccountKey=***')
        logger.info(f"Using connection string: {safe_conn_str}")
        
        sqlalchemy_url = create_sqlalchemy_url(connection_string)
        engine = create_engine(sqlalchemy_url, echo=False)

        if should_use_managed_identity():
            @event.listens_for(engine, "do_connect")
            def provide_token(dialect, conn_rec, cargs, cparams):
                try:
                    logger.info("Acquiring Azure AD token for database connection...")
                    credential = DefaultAzureCredential()
                    token = credential.get_token("https://database.windows.net/.default").token
                    token_bytes = token.encode("utf-16-le")
                    token_struct = struct.pack("=i", len(token_bytes)) + token_bytes
                    cparams["attrs_before"] = {1256: token_struct}  # SQL_COPT_SS_ACCESS_TOKEN
                    logger.info("Azure AD token injected successfully")
                except Exception as e:
                    logger.error(f"Failed to inject Azure AD token: {e}")
                    logger.error("Make sure Managed Identity is enabled and has access to the database")
                    raise

        SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
        
        # Register event listener for audit fields
        @event.listens_for(Session, "before_flush")
        def receive_before_flush(session, flush_context, instances):
            """Automatically populate audit fields before flush"""
            current_user = get_current_user()
            
            for obj in session.new:
                if hasattr(obj, 'CreatedBy'):
                    obj.CreatedBy = current_user
                if hasattr(obj, 'UpdatedBy'):
                    obj.UpdatedBy = current_user
            
            for obj in session.dirty:
                if hasattr(obj, 'UpdatedBy'):
                    obj.UpdatedBy = current_user
        
        with engine.connect() as conn:
            result = conn.execute("SELECT 1 as test").fetchone()
            logger.info(f"Database connection test successful: {result}")
        
        logger.info("Database initialized successfully with audit tracking")
    except Exception as e:
        logger.error(f"Error initializing database: {e}")
        if should_use_managed_identity():
            logger.error("Managed Identity troubleshooting:")
            logger.error("1. Ensure System Managed Identity is enabled on your Azure Function App")
            logger.error("2. Grant the Managed Identity 'db_datareader' and 'db_datawriter' roles on your SQL database")
            logger.error("3. For local development, ensure you're logged in with 'az login'")
        raise

def get_db():
    """Provide a database session"""
    if SessionLocal is None:
        initialize_database()
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

def create_tables():
    """Create database tables"""
    try:
        if engine is None:
            initialize_database()
        from models.database import Base
        Base.metadata.create_all(bind=engine)
        logger.info("Database tables created successfully")
    except Exception as e:
        logger.error(f"Error creating tables: {e}")
        raise

# Auto-initialize database on import
try:
    initialize_database()
except Exception as e:
    logger.warning(f"Database initialization failed on import: {e}")
