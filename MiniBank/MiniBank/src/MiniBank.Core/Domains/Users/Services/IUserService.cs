namespace MiniBank.Core.Domains.Users.Services;

public interface IUserService
{
    Task<User> GetByIdAsync(string id, CancellationToken token);

    Task<List<User>> GetAllUsersAsync(CancellationToken token);

    Task CreateAsync(User user, CancellationToken token);

    Task UpdateAsync(User user, CancellationToken token);

    Task DeleteByIdAsync(string id, CancellationToken token);
}