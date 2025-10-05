namespace HRMarket.Configuration.Exceptions;

public class AddInvoiceException(string message) : Exception(message)
{
}

public class NotFoundException(string message) : Exception(message)
{
}