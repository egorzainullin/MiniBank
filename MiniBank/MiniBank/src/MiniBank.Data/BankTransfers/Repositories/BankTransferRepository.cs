using Microsoft.EntityFrameworkCore;
using MiniBank.Core.Domains.BankTransfer;
using MiniBank.Core.Domains.BankTransfer.Repositories;

namespace MiniBank.Data.BankTransfers.Repositories;

public class BankTransferRepository : IBankTransferRepository
{
    private readonly DatabaseContext _context;

    public BankTransferRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(double amount, string currency, string fromAccountId, string toAccountId, CancellationToken token)
    {
        var entity = new BankTransferDbModel()
        {
            Id = Guid.NewGuid().ToString(),
            Amount = amount,
            Currency = currency,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId
        };
        await _context.BankTransfers.AddAsync(entity, token);
    }

    public Task<List<BankTransfer>> GetByIdFromAsync(string accountId,  CancellationToken token)
    {
        return _context.BankTransfers
            .Where(x => x.FromAccountId == accountId)
            .Select(x => new BankTransfer()
            {
                Id = x.Id,
                Currency = x.Currency,
                Amount = x.Amount,
                FromAccountId = x.FromAccountId,
                ToAccountId = x.ToAccountId
            }).ToListAsync(token);
    }
}