using HRMarket.Middleware;
using Npgsql;
using Stripe;

namespace HRMarket.Configuration.Exceptions;

public interface ISpecialExceptionsHandler
{
    /*public Task HandleStripeException(Exception ex, HttpContext context);
    public Task HandleAddInvoiceToDatabaseException(Exception ex, HttpContext context);*/
    Task HandlePostgresException(Exception ex, HttpContext context);
}

public class SpecialExceptionsHandler(ILogger<SpecialExceptionsHandler> logger) : ISpecialExceptionsHandler
{
    /*public Task HandleAddInvoiceToDatabaseException(Exception ex, HttpContext context)
    {
        var addInvoiceException = (AddInvoiceException)ex;
        logger.LogCritical("Retrying invoice addition for customer: {InvoiceCustomerId}",
            addInvoiceException.Invoice.CustomerId);
        return ExceptionHandlingMiddleware.WriteResponse(context, StatusCodes.Status500InternalServerError,
            $"Failed to add invoice for customer: {addInvoiceException.Invoice.CustomerId}");
    }

    public Task HandleStripeException(Exception ex, HttpContext context)
    {
        var stripeException = (StripeException)ex;
        logger.LogCritical("Stripe error occurred: {StripeErrorMessage}", stripeException.StripeError.Message);
        return ExceptionHandlingMiddleware.WriteResponse(context, StatusCodes.Status402PaymentRequired,
            "Payment processing error occurred.");
    }*/

    public Task HandlePostgresException(Exception ex, HttpContext context)
    {
        var pgEx = (PostgresException)ex;
        var message = pgEx.SqlState switch
        {
            "23505" =>
                $"Duplicate value violates unique constraint '{pgEx.ConstraintName}' on table '{pgEx.TableName}'. {pgEx.Detail}",
            "23503" =>
                $"Foreign key constraint '{pgEx.ConstraintName}' failed on table '{pgEx.TableName}'. {pgEx.Detail}",
            "23502" => $"Column '{pgEx.ColumnName}' in table '{pgEx.TableName}' cannot be null.",
            _ => $"Database error: {pgEx.MessageText}"
        };

        logger.LogWarning("Postgres error: {Code} - {Message}", pgEx.SqlState, pgEx.MessageText);

        return ExceptionHandlingMiddleware.WriteResponse(
            context,
            StatusCodes.Status400BadRequest,
            message,
            [pgEx.MessageText, pgEx.Detail!]
        );
    }
}