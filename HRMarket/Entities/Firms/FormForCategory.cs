using HRMarket.Entities.Answers;
using HRMarket.Entities.Categories;

namespace HRMarket.Entities.Firms;

public class FormForCategory
{ 
    public Guid FirmId { get; init; } 
    public Guid CategoryId { get; init; }
    
    public DateTime UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string? Message { get; set; }
    

    public Firm? Firm { get; init; } 

    public ICollection<Answer> Answers { get; set; } = [];
    public Category? Category { get; init; }
    
}