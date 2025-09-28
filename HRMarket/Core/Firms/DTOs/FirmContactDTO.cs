namespace HRMarket.Core.Firms.DTOs;

public class FirmContactDTO(string email, string phone) 
{
    private string Email { get; set; } = email;
    private string Phone { get; set; } = phone;
    
}