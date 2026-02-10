namespace DataSyncService.Exceptions;

/// <summary>
/// Exception thrown when a sync operation fails due to a foreign key constraint violation.
/// This typically means a referenced entity hasn't been synced yet.
/// The message should be retried after a delay.
/// </summary>
public class ForeignKeyConstraintException : Exception
{
    public ForeignKeyConstraintException(string message) : base(message)
    {
    }

    public ForeignKeyConstraintException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
