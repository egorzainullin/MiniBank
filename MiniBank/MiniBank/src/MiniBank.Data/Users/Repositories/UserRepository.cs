using Microsoft.EntityFrameworkCore;
using MiniBank.Core;
using MiniBank.Core.Domains.Users;
using MiniBank.Core.Domains.Users.Repositories;
using MiniBank.Core.Exceptions;

namespace MiniBank.Data.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _context;

    public UserRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<User> GetByIdAsync(string id, CancellationToken token)
    {
        var entity = await _context
            .Users.FirstOrDefaultAsync(it => it.Id == id, token);
        if (entity == null)
        {
            throw new ObjectNotFoundException($"User with id: {id} is not found");
        }

        return new User
        {
            Email = entity.Email,
            Id = entity.Id,
            Login = entity.Login
        };
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken token)
    {
        return _context.Users
            .Select(user => new User
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email
            }).ToListAsync(token);
    }

    public async Task CreateAsync(User user, CancellationToken token)
    {
        var entity = new UserDbModel
        {
            Id = Guid.NewGuid().ToString(),
            Login = user.Login,
            Email = user.Email
        };
        await _context.Users.AddAsync(entity, token);
    }

    public async Task UpdateAsync(User user, CancellationToken token)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(it => it.Id == user.Id, token);
        if (entity == null)
        {
            throw new ObjectNotFoundException($"User with id: {user.Id} is not found");
        }

        entity.Login = user.Login;
        entity.Email = user.Email;
    }

    public async Task DeleteByIdAsync(string id, CancellationToken token)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(it => it.Id == id, cancellationToken: token);
        if (entity == null)
        {
            throw new ObjectNotFoundException($"User with id: {id} is not found");
        }

        _context.Users.Remove(entity);
    }
}