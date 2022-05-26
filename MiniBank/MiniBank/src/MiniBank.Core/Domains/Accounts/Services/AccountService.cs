using MiniBank.Core.Currency;
using MiniBank.Core.DateTimeProvider;
using MiniBank.Core.Domains.Accounts.Repositories;
using MiniBank.Core.Domains.BankTransfer.Repositories;
using MiniBank.Core.Domains.Users.Repositories;
using MiniBank.Core.Exceptions;
using MiniBank.Core.UnitOfWork;

namespace MiniBank.Core.Domains.Accounts.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    private readonly IUserRepository _userRepository;

    private readonly IBankTransferRepository _bankTransferRepository;

    private readonly ICurrencyConverter _currencyConverter;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IDateTimeProvider _timeProvider;

    public AccountService(IAccountRepository accountRepository, IUserRepository userRepository,
        IBankTransferRepository bankTransferRepository, ICurrencyConverter currencyConverter, IUnitOfWork unitOfWork,
        IDateTimeProvider timeProvider)
    {
        _accountRepository = accountRepository;
        _userRepository = userRepository;
        _bankTransferRepository = bankTransferRepository;
        _currencyConverter = currencyConverter;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task CreateAsync(string userId, string currency, double amount, CancellationToken token)
    {
        var user = await _userRepository.GetByIdAsync(userId, token);
        if (user == null)
        {
            throw new ValidationException("User with this id does not exist");
        }

        if (!CurrencyConverter.IsAvailableCurrency(currency))
        {
            throw new ValidationException("Unknown currency");
        }

        await _accountRepository.CreateAsync(userId, currency, amount, _timeProvider.UtcNow(), token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public Task<List<Account>> GetAccountsByUserIdAsync(string userId, CancellationToken token)
    {
        return _accountRepository.GetAccountsByUserIdAsync(userId, token);
    }

    public async Task CloseAccountAsync(string id, CancellationToken token)
    {
        var account = await _accountRepository.GetByIdAsync(id, token);
        if (account == null)
        {
            throw new ObjectNotFoundException($"Account with id: {id} is not found");
        }

        if (!account.IsOpened)
        {
            throw new ValidationException("Closed twice");
        }

        if (account.Amount != 0)
        {
            throw new ValidationException("Amount is not 0");
        }

        await _accountRepository.CloseAccountAsync(id, _timeProvider.UtcNow(), token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task<double> CalculateCommissionAsync(double amount, string fromAccountId, string toAccountId,
        CancellationToken token)
    {
        var accountFrom = await _accountRepository.GetByIdAsync(fromAccountId, token);

        if (accountFrom == null)
        {
            throw new ObjectNotFoundException($"Account with id: {fromAccountId} is not found");
        }

        var accountTo = await _accountRepository.GetByIdAsync(toAccountId, token);
        if (accountTo == null)
        {
            throw new ObjectNotFoundException($"Account with id: {toAccountId} is not found");
        }

        if (accountFrom.UserId == accountTo.UserId)
        {
            return 0;
        }

        var commission = Math.Round(amount * 2) / 100;
        return commission;
    }

    public async Task TransferAsync(double amount, string fromAccountId, string toAccountId, CancellationToken token)
    {
        var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId, token);

        if (fromAccount == null)
        {
            throw new ObjectNotFoundException($"Object with id: {fromAccountId} are not found");
        }

        var toAccount = await _accountRepository.GetByIdAsync(toAccountId, token);

        if (toAccount == null)
        {
            throw new ObjectNotFoundException($"Object with id: {toAccountId} are not found");
        }

        if (!fromAccount.IsOpened)
        {
            throw new ValidationException($"Account with id: {fromAccountId} is closed");
        }

        if (!toAccount.IsOpened)
        {
            throw new ValidationException($"Account with id: {toAccountId} is closed");
        }

        if (amount > fromAccount.Amount)
        {
            throw new ValidationException("Amount is more than amount on account");
        }

        var commission = await CalculateCommissionAsync(amount, fromAccountId, toAccountId, token);
        await _accountRepository.UpdateAmountAsync(fromAccountId, fromAccount.Amount - amount, token);

        var newAmount = amount - commission;
        var toAmount = await _currencyConverter.ConvertFromToAsync(newAmount, fromAccount.Currency, toAccount.Currency);
        await _accountRepository.UpdateAmountAsync(toAccountId, toAccount.Amount + toAmount, token);

        await _bankTransferRepository.CreateAsync(amount, CurrencyConverter.ConvertToString(fromAccount.Currency),
            fromAccountId, toAccountId, token);
        await _unitOfWork.SaveChangesAsync(token);
    }
}