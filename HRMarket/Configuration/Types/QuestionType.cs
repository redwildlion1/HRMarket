// HRMarket/Configuration/Types/QuestionType.cs
namespace HRMarket.Configuration.Types;

/// <summary>
/// All supported question types
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Short text input (single line)
    /// </summary>
    String = 1,
    
    /// <summary>
    /// Long text input (multi-line)
    /// </summary>
    Text = 2,
    
    /// <summary>
    /// Numeric input
    /// </summary>
    Number = 3,
    
    /// <summary>
    /// Date input
    /// </summary>
    Date = 4,
    
    /// <summary>
    /// Single choice from options (radio buttons)
    /// </summary>
    SingleSelect = 5,
    
    /// <summary>
    /// Multiple choices from options (checkboxes)
    /// </summary>
    MultiSelect = 6
}