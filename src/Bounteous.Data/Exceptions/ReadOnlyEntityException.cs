namespace Bounteous.Data.Exceptions;

public class ReadOnlyEntityException : InvalidOperationException
{
    public ReadOnlyEntityException(string entityType, string operation)
        : base($"Cannot {operation} read-only entity '{entityType}'. This entity is marked as read-only and does not support Create, Update, or Delete operations.")
    {
    }
}
