namespace MiniBank.Core.Domains.Accounts.Repositories;

public interface IAccountRepository
{
    public Task CreateAsync(string userId, string currencyString, double amount, DateTime
        timeWhenOpened, CancellationToken token);

    public Task<List<Account>> GetAccountsByUserIdAsync(string userId, CancellationToken token);

    public Task<Account> GetByIdAsync(string id, CancellationToken token);

    public Task CloseAccountAsync(string id, DateTime timeWhenClosed, CancellationToken token);

    public Task<bool> HasAccountsAsync(string userId, CancellationToken token);

    public Task UpdateAmountAsync(string id, double amount, CancellationToken token);
}