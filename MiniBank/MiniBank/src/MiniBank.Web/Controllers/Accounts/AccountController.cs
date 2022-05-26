using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core;
using MiniBank.Core.Currency;
using MiniBank.Core.Domains.Accounts.Services;
using MiniBank.Web.Controllers.Accounts.Dto;

namespace MiniBank.Web.Controllers.Accounts;

[ApiController]
[Route("account")]
public class AccountController
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [Authorize]
    [HttpPost]
    public Task Create(string userId, string currency, double amount, CancellationToken token)
    {
        return _accountService.CreateAsync(userId, currency, amount, token);
    }

    [Authorize]
    [HttpPost("close")]
    public Task CloseAccount(string id, CancellationToken token)
    {
        return _accountService.CloseAccountAsync(id, token);
    }

    [Authorize]
    [HttpGet("calculate-commission")]
    public Task<double> CalculateCommission(double amount, string fromAccountId, string toAccountId, CancellationToken token)
    {
        return _accountService.CalculateCommissionAsync(amount, fromAccountId, toAccountId, token);
    }

    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<List<AccountDto>> GetAccountsByUserId(string userId, CancellationToken token)
    {
        var models = await _accountService.GetAccountsByUserIdAsync(userId, token);
        return models.Select(model =>
            new AccountDto()
            {
                Amount = model.Amount,
                Currency = CurrencyConverter.ConvertToString(model.Currency),
                DateWhenClosed = model.DateWhenClosed,
                DateWhenOpened = model.DateWhenOpened,
                Id = model.Id,
                IsOpened = model.IsOpened,
                UserId = model.UserId
            }
        ).ToList();
    }

    [Authorize]
    [HttpPost("transfer")]
    public Task Transfer(double amount, string fromAccountId, string toAccountId, CancellationToken token)
    {
        return _accountService.TransferAsync(amount, fromAccountId, toAccountId, token);
    }
}