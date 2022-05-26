namespace MiniBank.Core.Domains.Accounts;

public class Account
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public double Amount { get; set; }
    
    public Currency.Currency Currency { get; set; }

    public bool IsOpened { get; set; }

    public DateTime DateWhenOpened { get; set; }

    public DateTime DateWhenClosed { get; set; }
}