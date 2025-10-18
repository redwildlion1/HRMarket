namespace HRMarket.Configuration.Translation;

/// <summary>
/// Static class containing all validation error message keys
/// </summary>
public static class ValidationErrorKeys
{
    // General validation errors
    public const string Required = "Validation.Required";
    public const string MaxLength = "Validation.MaxLength";
    public const string MinLength = "Validation.MinLength";
    public const string EmailInvalid = "Validation.EmailInvalid";
    public const string Unique = "Validation.Unique";
    public const string InvalidUrl = "Validation.InvalidUrl";
    public const string InvalidEnum = "Validation.InvalidEnum";
    public const string MustBePositive = "Validation.MustBePositive";
    public const string MustBeGreaterThan = "Validation.MustBeGreaterThan";
    public const string MustBeLessThan = "Validation.MustBeLessThan";
    public const string InvalidRange = "Validation.InvalidRange";
    public const string InvalidDate = "Validation.InvalidDate";
    public const string DateMustBeInFuture = "Validation.DateMustBeInFuture";
    public const string DateMustBeInPast = "Validation.DateMustBeInPast";

    // Firm validation
    public static class Firm
    {
        public const string TypeInvalid = "Validation.Firm.TypeInvalid";
        public const string CuiInvalid = "Validation.Firm.CuiInvalid";
        public const string CuiFormat = "Validation.Firm.CuiFormat";
    }

    // Social media validation
    public static class SocialMedia
    {
        public const string LinkedInInvalid = "Validation.SocialMedia.LinkedInInvalid";
        public const string FacebookInvalid = "Validation.SocialMedia.FacebookInvalid";
        public const string TwitterInvalid = "Validation.SocialMedia.TwitterInvalid";
        public const string InstagramInvalid = "Validation.SocialMedia.InstagramInvalid";
    }

    // Question validation
    public static class Question
    {
        public const string TypeInvalid = "Validation.Question.TypeInvalid";
        public const string OptionsRequired = "Validation.Question.OptionsRequired";
        public const string OptionsMinCount = "Validation.Question.OptionsMinCount";
        public const string OptionsMaxCount = "Validation.Question.OptionsMaxCount";
        public const string OptionsShouldBeEmpty = "Validation.Question.OptionsShouldBeEmpty";
        public const string InvalidJsonSchema = "Validation.Question.InvalidJsonSchema";
        public const string DuplicateOrder = "Validation.Question.DuplicateOrder";
        public const string OrderGaps = "Validation.Question.OrderGaps";
        public const string VariantLanguageInvalid = "Validation.Question.VariantLanguageInvalid";
        public const string VariantDuplicateLanguage = "Validation.Question.VariantDuplicateLanguage";
    }

    // Option validation
    public static class Option
    {
        public const string DuplicateOrder = "Validation.Option.DuplicateOrder";
        public const string OrderGaps = "Validation.Option.OrderGaps";
    }

    // Answer validation
    public static class Answer
    {
        public const string QuestionNotFound = "Validation.Answer.QuestionNotFound";
        public const string ResponseInvalid = "Validation.Answer.ResponseInvalid";
        public const string OptionNotFound = "Validation.Answer.OptionNotFound";
        public const string SchemaValidationFailed = "Validation.Answer.SchemaValidationFailed";
    }

    // Auth validation
    public static class Auth
    {
        public const string PasswordTooShort = "Validation.Auth.PasswordTooShort";
        public const string PasswordRequiresDigit = "Validation.Auth.PasswordRequiresDigit";
        public const string PasswordRequiresLowercase = "Validation.Auth.PasswordRequiresLowercase";
        public const string PasswordRequiresUppercase = "Validation.Auth.PasswordRequiresUppercase";
        public const string PasswordRequiresNonAlphanumeric = "Validation.Auth.PasswordRequiresNonAlphanumeric";
    }
}