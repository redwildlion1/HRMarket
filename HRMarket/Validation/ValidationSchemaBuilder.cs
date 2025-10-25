using System.Text.Json;
using HRMarket.Configuration.Types;

namespace HRMarket.Validation;

public static class ValidationSchemaBuilder
{
    public static string BuildStringSchema(
        int? minLength = null,
        int? maxLength = null,
        string? pattern = null,
        List<string>? allowedValues = null)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "string"
        };
        
        if (minLength.HasValue)
            schema["minLength"] = minLength.Value;
        
        if (maxLength.HasValue)
            schema["maxLength"] = maxLength.Value;
        
        if (!string.IsNullOrEmpty(pattern))
            schema["pattern"] = pattern;
        
        if (allowedValues is { Count: > 0 })
            schema["enum"] = allowedValues;
        
        return JsonSerializer.Serialize(schema);
    }
    
    public static string BuildNumberSchema(
        double? minimum = null,
        double? maximum = null,
        bool? isInteger = null,
        double? multipleOf = null)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = isInteger == true ? "integer" : "number"
        };
        
        if (minimum.HasValue)
            schema["minimum"] = minimum.Value;
        
        if (maximum.HasValue)
            schema["maximum"] = maximum.Value;
        
        if (multipleOf.HasValue)
            schema["multipleOf"] = multipleOf.Value;
        
        return JsonSerializer.Serialize(schema);
    }
    
    public static string BuildDateSchema(
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool allowPast = true,
        bool allowFuture = true)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "string",
            ["format"] = "date"
        };
        
        if (minDate.HasValue)
            schema["minimum"] = minDate.Value.ToString("yyyy-MM-dd");
        
        if (maxDate.HasValue)
            schema["maximum"] = maxDate.Value.ToString("yyyy-MM-dd");
        
        if (!allowPast)
            schema["minimum"] = DateTime.UtcNow.ToString("yyyy-MM-dd");
        
        if (!allowFuture)
            schema["maximum"] = DateTime.UtcNow.ToString("yyyy-MM-dd");
        
        return JsonSerializer.Serialize(schema);
    }
    
    public static string BuildChoiceSchema(
        QuestionType type,
        int minSelections = 1,
        int? maxSelections = null)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "array",
            ["minItems"] = type == QuestionType.SingleSelect ? 1 : minSelections
        };
        
        if (type == QuestionType.SingleSelect)
        {
            schema["maxItems"] = 1;
        }
        else if (maxSelections.HasValue)
        {
            schema["maxItems"] = maxSelections.Value;
        }
        
        schema["items"] = new Dictionary<string, object>
        {
            ["type"] = "string",
            ["format"] = "uuid"
        };
        
        return JsonSerializer.Serialize(schema);
    }
}