using MiniBank.Core.Domains.BankTransfer.Repositories;
using MiniBank.Core.UnitOfWork;

namespace MiniBank.Core.Domains.BankTransfer.Services;

public class BankTransferService : IBankTransferService
{
    private readonly IBankTransferRepository _bankTransferRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public BankTransferService(IBankTransferRepository bankTransferRepository, IUnitOfWork unitOfWork)
    {
        _bankTransferRepository = bankTransferRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<List<BankTransfer>> GetByAccountIdFromAsync(string fromAccountId, CancellationToken token)
    {
        return _bankTransferRepository.GetByIdFromAsync(fromAccountId, token);
    }
}