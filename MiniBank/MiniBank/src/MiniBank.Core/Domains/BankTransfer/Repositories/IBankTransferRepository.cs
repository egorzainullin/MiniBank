namespace MiniBank.Core.Domains.BankTransfer.Repositories;

public interface IBankTransferRepository
{
    public Task CreateAsync(double amount, string currency, string fromAccountId, string toAccountId, CancellationToken token);
    
    Task<List<BankTransfer>> GetByIdFromAsync(string accountId, CancellationToken token);
}