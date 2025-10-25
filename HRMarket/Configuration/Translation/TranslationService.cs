using System.Collections.Concurrent;

namespace HRMarket.Configuration.Translation;

public interface ITranslationService
{
    string Translate(string key, string language = "ro", params object[] args);
    string TranslateValidationError(string errorKey, string language = "ro", params object[] args);
    Task<string> TranslateTextAsync(string value, string requestLanguage, string english);
}

public class TranslationService : ITranslationService
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _translations = new();
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(ILogger<TranslationService> logger)
    {
        _logger = logger;
        LoadTranslations();
    }

    public string Translate(string key, string language = SupportedLanguages.English, params object[] args)
    {
        if (!_translations.TryGetValue(language, out var translations))
        {
            _logger.LogWarning("Language {Language} not found, falling back to English", language);
            language = SupportedLanguages.English;
            _translations.TryGetValue(language, out translations);
        }

        if (translations == null || !translations.TryGetValue(key, out var translation))
        {
            _logger.LogWarning("Translation key {Key} not found for language {Language}", key, language);
            return key;
        }

        try
        {
            return args.Length > 0 ? string.Format(translation, args) : translation;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Failed to format translation for key {Key}", key);
            return translation;
        }
    }

    public string TranslateValidationError(string errorKey, string language = SupportedLanguages.English, params object[] args)
    {
        return Translate(errorKey, language, args);
    }

    public Task<string> TranslateTextAsync(string value, string requestLanguage, string english)
    {
        throw new NotImplementedException();    
    }

    private void LoadTranslations()
    {
        // Romanian translations
        _translations[SupportedLanguages.Romanian] = new Dictionary<string, string>
        {
            // General validation errors
            [ValidationErrorKeys.Required] = "Câmpul \"{0}\" este obligatoriu.",
            [ValidationErrorKeys.MaxLength] = "Câmpul \"{0}\" nu poate depăși {1} caractere.",
            [ValidationErrorKeys.MinLength] = "Câmpul \"{0}\" trebuie să conțină cel puțin {1} caractere.",
            [ValidationErrorKeys.EmailInvalid] = "Vă rugăm să introduceți o adresă de email validă.",
            [ValidationErrorKeys.Unique] = "Câmpul \"{0}\" trebuie să fie unic. Valoarea introdusă există deja.",
            [ValidationErrorKeys.InvalidUrl] = "Vă rugăm să introduceți un URL valid.",
            [ValidationErrorKeys.InvalidEnum] = "Valoarea selectată pentru \"{0}\" nu este validă.",
            [ValidationErrorKeys.MustBePositive] = "Câmpul \"{0}\" trebuie să fie un număr pozitiv.",
            [ValidationErrorKeys.MustBeGreaterThan] = "Câmpul \"{0}\" trebuie să fie mai mare decât {1}.",
            [ValidationErrorKeys.MustBeLessThan] = "Câmpul \"{0}\" trebuie să fie mai mic decât {1}.",
            [ValidationErrorKeys.InvalidRange] = "Câmpul \"{0}\" trebuie să fie între {1} și {2}.",
            [ValidationErrorKeys.InvalidDate] = "Câmpul \"{0}\" conține o dată invalidă.",
            [ValidationErrorKeys.DateMustBeInFuture] = "Câmpul \"{0}\" trebuie să fie o dată în viitor.",
            [ValidationErrorKeys.DateMustBeInPast] = "Câmpul \"{0}\" trebuie să fie o dată în trecut.",
            [ValidationErrorKeys.InvalidFormat] = "{0} are un format invalid",
            [ValidationErrorKeys.MinValue] = "{0} trebuie să fie cel puțin {1}",
            [ValidationErrorKeys.MaxValue] = "{0} trebuie să fie cel mult {1}",
            [ValidationErrorKeys.InvalidOption] = "Una sau mai multe opțiuni selectate sunt invalide",
            [ValidationErrorKeys.MinSelections] = "Trebuie să selectați cel puțin {0} opțiune/opțiuni",
            [ValidationErrorKeys.MaxSelections] = "Puteți selecta cel mult {0} opțiune/opțiuni",

            // Firm validation
            [ValidationErrorKeys.Firm.TypeInvalid] = "Tipul firmei nu este valid.",
            [ValidationErrorKeys.Firm.CuiInvalid] = "CUI-ul trebuie să conțină exact 12 caractere.",
            [ValidationErrorKeys.Firm.CuiFormat] = "CUI-ul trebuie să conțină doar cifre.",

            // Social media validation
            [ValidationErrorKeys.SocialMedia.LinkedInInvalid] =
                "Vă rugăm să introduceți un URL valid de LinkedIn (linkedin.com).",
            [ValidationErrorKeys.SocialMedia.FacebookInvalid] =
                "Vă rugăm să introduceți un URL valid de Facebook (facebook.com).",
            [ValidationErrorKeys.SocialMedia.TwitterInvalid] =
                "Vă rugăm să introduceți un URL valid de Twitter (twitter.com sau x.com).",
            [ValidationErrorKeys.SocialMedia.InstagramInvalid] =
                "Vă rugăm să introduceți un URL valid de Instagram (instagram.com).",

            // Question validation
            [ValidationErrorKeys.Question.TypeInvalid] = "Tipul întrebării nu este valid.",
            [ValidationErrorKeys.Question.OptionsRequired] = "Întrebările de tip \"{0}\" trebuie să conțină opțiuni.",
            [ValidationErrorKeys.Question.OptionsMinCount] =
                "Întrebările de tip \"{0}\" trebuie să conțină cel puțin {1} opțiuni.",
            [ValidationErrorKeys.Question.OptionsMaxCount] =
                "Întrebările de tip \"{0}\" pot conține maxim {1} opțiuni.",
            [ValidationErrorKeys.Question.OptionsShouldBeEmpty] = "Întrebările de tip \"{0}\" nu pot conține opțiuni.",
            [ValidationErrorKeys.Question.InvalidJsonSchema] = "Schema JSON de validare nu este validă.",
            [ValidationErrorKeys.Question.DuplicateOrder] = "Există ordini duplicate în lista de întrebări.",
            [ValidationErrorKeys.Question.OrderGaps] = "Există goluri în ordinea întrebărilor.",
            [ValidationErrorKeys.Question.VariantLanguageInvalid] = "Limba \"{0}\" nu este validă pentru variant.",
            [ValidationErrorKeys.Question.VariantDuplicateLanguage] =
                "Varianta pentru limba \"{0}\" este definită de mai multe ori.",

            // Option validation
            [ValidationErrorKeys.Option.DuplicateOrder] = "Există ordini duplicate în lista de opțiuni.",
            [ValidationErrorKeys.Option.OrderGaps] = "Există goluri în ordinea opțiunilor.",

            // Answer validation
            [ValidationErrorKeys.Answer.QuestionNotFound] = "Întrebarea cu ID-ul \"{0}\" nu există.",
            [ValidationErrorKeys.Answer.ResponseInvalid] = "Răspunsul nu corespunde tipului întrebării.",
            [ValidationErrorKeys.Answer.OptionNotFound] = "Opțiunea selectată nu este validă pentru această întrebare.",
            [ValidationErrorKeys.Answer.SchemaValidationFailed] = "Răspunsul nu respectă schema de validare.",

            // Auth validation
            [ValidationErrorKeys.Auth.PasswordTooShort] = "Parola trebuie să aibă cel puțin {0} caractere.",
            [ValidationErrorKeys.Auth.PasswordRequiresDigit] = "Parola trebuie să conțină cel puțin o cifră.",
            [ValidationErrorKeys.Auth.PasswordRequiresLowercase] = "Parola trebuie să conțină cel puțin o literă mică.",
            [ValidationErrorKeys.Auth.PasswordRequiresUppercase] = "Parola trebuie să conțină cel puțin o literă mare.",
            [ValidationErrorKeys.Auth.PasswordRequiresNonAlphanumeric] =
                "Parola trebuie să conțină cel puțin un caracter special.",
            
            // Identity-specific errors
            [ValidationErrorKeys.Auth.DuplicateEmail] = "Adresa de email este deja înregistrată.",
            [ValidationErrorKeys.Auth.DuplicateUserName] = "Numele de utilizator este deja folosit.",
            [ValidationErrorKeys.Auth.InvalidEmail] = "Adresa de email nu este validă.",
            [ValidationErrorKeys.Auth.InvalidUserName] = "Numele de utilizator nu este valid.",
            [ValidationErrorKeys.Auth.InvalidToken] = "Token-ul de securitate nu este valid.",
            [ValidationErrorKeys.Auth.UserNotFound] = "Utilizatorul nu a fost găsit.",
            [ValidationErrorKeys.Auth.UserAlreadyHasPassword] = "Utilizatorul are deja o parolă setată.",
            [ValidationErrorKeys.Auth.UserLockoutNotEnabled] = "Blocarea contului nu este activată pentru acest utilizator.",
            [ValidationErrorKeys.Auth.UserAlreadyInRole] = "Utilizatorul este deja în acest rol.",
            [ValidationErrorKeys.Auth.UserNotInRole] = "Utilizatorul nu este în acest rol.",
            [ValidationErrorKeys.Auth.PasswordMismatch] = "Parola este incorectă.",
            [ValidationErrorKeys.Auth.DefaultError] = "A apărut o eroare. Vă rugăm să încercați din nou.",
            [ValidationErrorKeys.Auth.ConcurrencyFailure] = "Eroare de concurență. Vă rugăm să reîncărcați și să încercați din nou.",
            [ValidationErrorKeys.Auth.LoginAlreadyAssociated] = "Acest login este deja asociat cu alt cont.",
            [ValidationErrorKeys.Auth.InvalidRoleName] = "Numele rolului nu este valid.",
            [ValidationErrorKeys.Auth.DuplicateRoleName] = "Numele rolului este deja folosit.",
            [ValidationErrorKeys.Auth.RecoveryCodeRedemptionFailed] = "Codul de recuperare nu este valid.",
            
            // Custom auth errors
            [ValidationErrorKeys.Auth.InvalidCredentials] = "Email sau parolă invalidă.",
            [ValidationErrorKeys.Auth.EmailNotConfirmed] = "Email-ul nu a fost confirmat. Vă rugăm să verificați căsuța de email.",
            [ValidationErrorKeys.Auth.InvalidRefreshToken] = "Token-ul de reîmprospătare nu este valid.",
        };


        // English translations
        _translations[SupportedLanguages.English] = new Dictionary<string, string>
        {
            // General validation errors
            [ValidationErrorKeys.Required] = "The field \"{0}\" is required.",
            [ValidationErrorKeys.MaxLength] = "The field \"{0}\" cannot exceed {1} characters.",
            [ValidationErrorKeys.MinLength] = "The field \"{0}\" must contain at least {1} characters.",
            [ValidationErrorKeys.EmailInvalid] = "Please enter a valid email address.",
            [ValidationErrorKeys.Unique] = "The field \"{0}\" must be unique. The entered value already exists.",
            [ValidationErrorKeys.InvalidUrl] = "Please enter a valid URL.",
            [ValidationErrorKeys.InvalidEnum] = "The selected value for \"{0}\" is not valid.",
            [ValidationErrorKeys.MustBePositive] = "The field \"{0}\" must be a positive number.",
            [ValidationErrorKeys.MustBeGreaterThan] = "The field \"{0}\" must be greater than {1}.",
            [ValidationErrorKeys.MustBeLessThan] = "The field \"{0}\" must be less than {1}.",
            [ValidationErrorKeys.InvalidRange] = "The field \"{0}\" must be between {1} and {2}.",
            [ValidationErrorKeys.InvalidDate] = "The field \"{0}\" contains an invalid date.",
            [ValidationErrorKeys.DateMustBeInFuture] = "The field \"{0}\" must be a date in the future.",
            [ValidationErrorKeys.DateMustBeInPast] = "The field \"{0}\" must be a date in the past.",
            [ValidationErrorKeys.InvalidFormat] = "{0} has an invalid format",
            [ValidationErrorKeys.MinValue] = "{0} must be at least {1}",
            [ValidationErrorKeys.MaxValue] = "{0} must be at most {1}",
            [ValidationErrorKeys.InvalidOption] = "One or more selected options are invalid",
            [ValidationErrorKeys.MinSelections] = "You must select at least {0} option(s)",
            [ValidationErrorKeys.MaxSelections] = "You can select at most {0} option(s)",
            
        

            // Firm validation
            [ValidationErrorKeys.Firm.TypeInvalid] = "The firm type is not valid.",
            [ValidationErrorKeys.Firm.CuiInvalid] = "The CUI must contain exactly 12 characters.",
            [ValidationErrorKeys.Firm.CuiFormat] = "The CUI must contain only digits.",

            // Social media validation
            [ValidationErrorKeys.SocialMedia.LinkedInInvalid] = "Please enter a valid LinkedIn URL (linkedin.com).",
            [ValidationErrorKeys.SocialMedia.FacebookInvalid] = "Please enter a valid Facebook URL (facebook.com).",
            [ValidationErrorKeys.SocialMedia.TwitterInvalid] =
                "Please enter a valid Twitter URL (twitter.com or x.com).",
            [ValidationErrorKeys.SocialMedia.InstagramInvalid] = "Please enter a valid Instagram URL (instagram.com).",

            // Question validation
            [ValidationErrorKeys.Question.TypeInvalid] = "The question type is not valid.",
            [ValidationErrorKeys.Question.OptionsRequired] = "Questions of type \"{0}\" must contain options.",
            [ValidationErrorKeys.Question.OptionsMinCount] =
                "Questions of type \"{0}\" must contain at least {1} options.",
            [ValidationErrorKeys.Question.OptionsMaxCount] =
                "Questions of type \"{0}\" can contain at most {1} options.",
            [ValidationErrorKeys.Question.OptionsShouldBeEmpty] = "Questions of type \"{0}\" cannot contain options.",
            [ValidationErrorKeys.Question.InvalidJsonSchema] = "The validation JSON schema is not valid.",
            [ValidationErrorKeys.Question.DuplicateOrder] = "There are duplicate orders in the question list.",
            [ValidationErrorKeys.Question.OrderGaps] = "There are gaps in the question order.",
            [ValidationErrorKeys.Question.VariantLanguageInvalid] =
                "The language \"{0}\" is not valid for the variant.",
            [ValidationErrorKeys.Question.VariantDuplicateLanguage] =
                "The variant for language \"{0}\" is defined multiple times.",

            // Option validation
            [ValidationErrorKeys.Option.DuplicateOrder] = "There are duplicate orders in the option list.",
            [ValidationErrorKeys.Option.OrderGaps] = "There are gaps in the option order.",

            // Answer validation
            [ValidationErrorKeys.Answer.QuestionNotFound] = "The question with ID \"{0}\" does not exist.",
            [ValidationErrorKeys.Answer.ResponseInvalid] = "The response does not match the question type.",
            [ValidationErrorKeys.Answer.OptionNotFound] = "The selected option is not valid for this question.",
            [ValidationErrorKeys.Answer.SchemaValidationFailed] =
                "The response does not comply with the validation schema.",

            // Auth validation
            [ValidationErrorKeys.Auth.PasswordTooShort] = "Password must be at least {0} characters long.",
            [ValidationErrorKeys.Auth.PasswordRequiresDigit] = "Password must contain at least one digit.",
            [ValidationErrorKeys.Auth.PasswordRequiresLowercase] =
                "Password must contain at least one lowercase letter.",
            [ValidationErrorKeys.Auth.PasswordRequiresUppercase] =
                "Password must contain at least one uppercase letter.",
            [ValidationErrorKeys.Auth.PasswordRequiresNonAlphanumeric] =
                "Password must contain at least one special character.",
            
            // Identity-specific errors
            [ValidationErrorKeys.Auth.DuplicateEmail] = "Email address is already registered.",
            [ValidationErrorKeys.Auth.DuplicateUserName] = "Username is already taken.",
            [ValidationErrorKeys.Auth.InvalidEmail] = "Email address is not valid.",
            [ValidationErrorKeys.Auth.InvalidUserName] = "Username is not valid.",
            [ValidationErrorKeys.Auth.InvalidToken] = "Security token is not valid.",
            [ValidationErrorKeys.Auth.UserNotFound] = "User was not found.",
            [ValidationErrorKeys.Auth.UserAlreadyHasPassword] = "User already has a password set.",
            [ValidationErrorKeys.Auth.UserLockoutNotEnabled] = "Lockout is not enabled for this user.",
            [ValidationErrorKeys.Auth.UserAlreadyInRole] = "User is already in this role.",
            [ValidationErrorKeys.Auth.UserNotInRole] = "User is not in this role.",
            [ValidationErrorKeys.Auth.PasswordMismatch] = "Password is incorrect.",
            [ValidationErrorKeys.Auth.DefaultError] = "An error occurred. Please try again.",
            [ValidationErrorKeys.Auth.ConcurrencyFailure] = "Concurrency error. Please reload and try again.",
            [ValidationErrorKeys.Auth.LoginAlreadyAssociated] = "This login is already associated with another account.",
            [ValidationErrorKeys.Auth.InvalidRoleName] = "Role name is not valid.",
            [ValidationErrorKeys.Auth.DuplicateRoleName] = "Role name is already in use.",
            [ValidationErrorKeys.Auth.RecoveryCodeRedemptionFailed] = "Recovery code is not valid.",
        };
    }
}