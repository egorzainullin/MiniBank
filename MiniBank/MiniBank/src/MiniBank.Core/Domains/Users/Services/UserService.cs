using MiniBank.Core.Domains.Accounts.Repositories;
using MiniBank.Core.Domains.Users.Repositories;
using MiniBank.Core.Exceptions;
using MiniBank.Core.UnitOfWork;

namespace MiniBank.Core.Domains.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    
    private readonly IAccountRepository _accountRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<User> GetByIdAsync(string id, CancellationToken token)
    {
        return _userRepository.GetByIdAsync(id, token);
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken token)
    {
        return _userRepository.GetAllUsersAsync(token);
    }

    public async Task CreateAsync(User user, CancellationToken token)
    {
        var users = await _userRepository.GetAllUsersAsync(token);
        if (users.Select(u => u.Login).Contains(user.Login))
        {
            throw new ValidationException($"User with login: {user.Login} already exists");
        }
        await _userRepository.CreateAsync(user, token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task UpdateAsync(User user, CancellationToken token)
    {
        await _userRepository.UpdateAsync(user, token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task DeleteByIdAsync(string id, CancellationToken token)
    {
        if (await _accountRepository.HasAccountsAsync(id, token))
        {
            throw new ValidationException("This user has accounts");
        }
        await _userRepository.DeleteByIdAsync(id, token);
        await _unitOfWork.SaveChangesAsync(token);
    }
}