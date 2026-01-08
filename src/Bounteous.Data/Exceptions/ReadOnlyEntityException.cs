namespace Bounteous.Data.Exceptions;

public class ReadOnlyEntityException(string entityType, string operation) : InvalidOperationException(
    $"Cannot {operation} read-only entity '{entityType}'. This entity is marked as read-only and does not support Create, Update, or Delete operations.");
