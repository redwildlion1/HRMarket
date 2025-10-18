namespace HRMarket.Configuration.Exceptions;

public class AddInvoiceException(string message) : Exception(message)
{
}

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public NotFoundException(string message) : base(message)
    {
        ResourceType = string.Empty;
        ResourceId = string.Empty;
    }

    public NotFoundException(string resourceType, string resourceId) 
        : base($"{resourceType} with id '{resourceId}' was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string resourceType, string resourceId, string message) 
        : base(message)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}