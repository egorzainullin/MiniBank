using Microsoft.EntityFrameworkCore;
using MiniBank.Core;
using MiniBank.Core.Currency;
using MiniBank.Core.Domains.Accounts;
using MiniBank.Core.Domains.Accounts.Repositories;
using MiniBank.Core.Domains.Users;
using MiniBank.Core.Exceptions;

namespace MiniBank.Data.Accounts.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly DatabaseContext _context;

    public AccountRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(string userId, string currencyString, double amount, DateTime dateWhenOpened,
        CancellationToken token)
    {
        var account = new AccountDbModel()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Amount = amount,
            Currency = currencyString,
            DateWhenOpened = dateWhenOpened,
            IsOpened = true
        };
        await _context.Accounts.AddAsync(account, token);
    }

    public Task<List<Account>> GetAccountsByUserIdAsync(string userId, CancellationToken token)
    {
        return
            _context.Accounts
                .Where(it => it.UserId == userId)
                .Select(account =>
                    new Account()
                    {
                        Amount = account.Amount,
                        Currency = CurrencyConverter.ConvertFromString(account.Currency),
                        DateWhenClosed = account.DateWhenClosed,
                        DateWhenOpened = account.DateWhenOpened,
                        Id = account.Id,
                        IsOpened = account.IsOpened,
                        UserId = account.UserId
                    }
                ).ToListAsync(token);
    }

    public async Task<Account> GetByIdAsync(string id, CancellationToken token)
    {
        var entity = await _context.Accounts.FirstOrDefaultAsync(t => t.Id == id, token);
        if (entity == null)
        {
            return null;
        }

        return new Account()
        {
            Amount = entity.Amount,
            Currency = CurrencyConverter.ConvertFromString(entity.Currency),
            DateWhenClosed = entity.DateWhenClosed,
            DateWhenOpened = entity.DateWhenOpened,
            Id = entity.Id,
            IsOpened = entity.IsOpened,
            UserId = entity.UserId
        };
    }

    public async Task CloseAccountAsync(string id, DateTime dateWhenClosed, CancellationToken token)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(it => it.Id == id, token);
        if (account != null)
        {
            account.IsOpened = false;
            account.DateWhenClosed = dateWhenClosed;
        }
        else
        {
            throw new ObjectNotFoundException($"Account with id: {id} is not found");
        }
    }

    public Task<bool> HasAccountsAsync(string userId, CancellationToken token)
    {
        return _context.Accounts.AnyAsync(it => it.UserId == userId, token);
    }

    public async Task UpdateAmountAsync(string id, double amount, CancellationToken token)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(it => it.Id == id, cancellationToken: token);
        if (account == null)
        {
            throw new ObjectNotFoundException($"Account with this id: {id} is not found");
        }

        account.Amount = amount;
    }
}