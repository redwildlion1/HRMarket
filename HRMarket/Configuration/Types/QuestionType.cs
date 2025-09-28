namespace HRMarket.Configuration.Types;

public enum QuestionType
{
    String,
    Text,
    Number,
    Date,
    SingleSelect,
    MultiSelect
}

public static class QuestionTypeExtensions
{
    private static readonly Dictionary<QuestionType, Type> TypeMap = new()
    {
        { QuestionType.String, typeof(string) },
        { QuestionType.Text, typeof(string) },
        { QuestionType.Number, typeof(int) },
        { QuestionType.Date, typeof(DateOnly) },
    };

    private static void IsValidValue(QuestionType questionType, object? value)
    {
        if (!TypeMap.TryGetValue(questionType, out var expectedType))
            throw new ArgumentException($"Unrecognized QuestionType: {questionType}");
        if (value != null && !expectedType.IsInstanceOfType(value))
            throw new ArgumentException($"Value does not match expected type {expectedType.Name} for {questionType}");
    }

    public static void IsValidValueByTypeString(string typeString, object? value)
    {
        if (!Enum.TryParse<QuestionType>(typeString, ignoreCase: true, out var questionType))
            throw new ArgumentException($"Unrecognized type string: {typeString}");
        IsValidValue(questionType, value);
    }
}