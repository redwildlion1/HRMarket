namespace Common.Email;

public class EmailQueueSettings 
{
    public const string SectionName = "EmailQueueSettings";
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Host { get; set; }
    public required string QueueName { get; set; }
}
