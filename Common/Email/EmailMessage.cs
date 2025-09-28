namespace Common.Email
{
    public class EmailMessage
    {
        public required string ToEmail { get; init; }
        public required string Subject { get; init; }
        public required string HtmlBody { get; init; }
    }
}