using HRMarket.Entities.Answers;

namespace HRMarket.Entities.Firms;

public class FormSubmission
{ 

    public Guid FirmId { get; init; } 
    
    public DateTime UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string? Message { get; set; }
    

    public Firm? Firm { get; init; } 

    public ICollection<Answer> Answers { get; set; } = [];
    
}