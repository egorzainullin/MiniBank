namespace MiniBank.Core.Domains.Accounts.Services;

public interface IAccountService
{
    public Task CreateAsync(string userId, string currency, double amount, CancellationToken token);
    
    public Task<List<Account>> GetAccountsByUserIdAsync(string userId, CancellationToken token);

    public Task CloseAccountAsync(string id, CancellationToken token);

    public Task<double> CalculateCommissionAsync(double amount, string fromAccountId, string toAccountId, CancellationToken token);

    public Task TransferAsync(double amount, string fromAccountId, string toAccountId, CancellationToken token);
}