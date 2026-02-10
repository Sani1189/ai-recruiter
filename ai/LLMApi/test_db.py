"""
Test script for Azure SQL connection using Managed Identity
"""

from config import settings
from database import initialize_database, get_db
from sqlalchemy.sql import text

def test_sql_connection():
    try:
        # Initialize database engine
        initialize_database()
        # Get a session
        db_session = next(get_db())

        # Test query
        result = db_session.execute(text("SELECT TOP 1 * FROM Prompts")).fetchall()
        print("✅ Connection successful. Result:", result)

    except Exception as e:
        print("❌ Connection failed:", str(e))

    finally:
        # Close the session
        if 'db_session' in locals():
            db_session.close()


if __name__ == "__main__":
    test_sql_connection()
