namespace HRMarket.OuterAPIs.Email;

public static class EmailTemplates
{
    public static string ConfirmationEmailSubject => "Please confirm your email address";

    public static string GetConfirmationEmailBody(string confirmationLink)
    {
        return $"""

                        <html>
                            <body>
                                <h1>Welcome to HRMarket!</h1>
                                <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
                                <a href='{confirmationLink}'>Confirm Email</a>
                                <p>If you did not register, please ignore this email.</p>
                            </body>
                        </html>
                """;
    }
}