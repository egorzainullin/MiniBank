using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core.Domains.BankTransfer.Services;
using MiniBank.Web.Controllers.BankTransfers.Dto;

namespace MiniBank.Web.Controllers.BankTransfers;

[ApiController]
[Route("bank-transfer")]
public class BankTransferController
{
    private readonly IBankTransferService _bankTransferService;

    public BankTransferController(IBankTransferService bankTransferService)
    {
        _bankTransferService = bankTransferService;
    }
    
    [Authorize]
    [HttpGet("account/from/{fromAccountId}")]
    public async Task<List<BankTransferDto>> GetByAccountIdFromTransfersCome(string fromAccountId, CancellationToken token)
    {
        var models = await _bankTransferService.GetByAccountIdFromAsync(fromAccountId, token);
        return models.Select(model => 
            new BankTransferDto()
            {
                Id = model.Id,
                Amount = model.Amount,
                Currency = model.Currency,
                FromAccountId = model.FromAccountId,
                ToAccountId = model.ToAccountId
            }
        ).ToList();
    }
}