namespace MiniBank.Core.Domains.BankTransfer.Services;

public interface IBankTransferService
{
    public Task<List<BankTransfer>> GetByAccountIdFromAsync(string fromAccountId, CancellationToken token);

}