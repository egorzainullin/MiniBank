namespace MiniBank.Web.Controllers.Accounts.Dto;

public class AccountDto
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public double Amount { get; set; }
    
    public string Currency { get; set; }

    public bool IsOpened { get; set; }

    public DateTime DateWhenOpened { get; set; }

    public DateTime DateWhenClosed { get; set; }
}