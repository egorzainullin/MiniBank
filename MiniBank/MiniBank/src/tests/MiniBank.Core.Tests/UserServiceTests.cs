using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MiniBank.Core.Domains.Accounts.Repositories;
using MiniBank.Core.Domains.Users;
using MiniBank.Core.Domains.Users.Repositories;
using MiniBank.Core.Domains.Users.Services;
using MiniBank.Core.Exceptions;
using MiniBank.Core.UnitOfWork;
using Xunit;
using Moq;

namespace MiniBank.Core.Tests;

public class UserServiceTests
{
    private readonly IUserService _service;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _accountRepoMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UserService(_userRepoMock.Object, _accountRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetUserById_SuccessPath_ReturnUserWithThisId()
    {
        // Arrange
        const string email = "email@mail.ru";
        const string login = "login1";
        _userRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ident, CancellationToken _) => new User { Id = ident, Email = email, Login = login });
        const string id = "id1";
        
        // Act
        var user = await _service.GetByIdAsync(id, CancellationToken.None);
        
        // Assert
        Assert.Equal(id, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(login, user.Login);
    }

    [Fact]
    public async Task GetUserById_WithIdThatDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => null);
        const string id = "id1";
        
        // Act
        var user = await _service.GetByIdAsync(id, CancellationToken.None);
        
        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task GetAllUsers_SuccessPath_ReturnsAllUsers()
    {
        // Arrange
        const string id1 = "id1";
        const string id2 = "id2";
        _userRepoMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken _) => new List<User>
            {
                new() { Id = id1, Email = "", Login = "" },
                new() { Id = id2, Email = "", Login = "" }
            });
        
        // Act
        var users = (await _service.GetAllUsersAsync(CancellationToken.None)).ToList();
        
        // Assert
        Assert.Contains<User>(users, x => x.Id == id1);
        Assert.Contains<User>(users, x => x.Id == id2);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task CreateUser_SuccessPath_CreatesUser()
    {
        // Arrange
        var user = new User { Login = "login", Id = "id", Email = "email" };
        _userRepoMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken _) => new List<User>());
        
        // Act
        await _service.CreateAsync(user, CancellationToken.None);
        
        // Assert
        _userRepoMock.Verify(repo => repo.CreateAsync(It.Is<User>(u => u.Login == user.Login && u.Email == user.Email),
            CancellationToken.None));
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task CreateUser_WithLoginThatExists_ThrowsValidationException()
    {
        // Arrange
        var user = new User { Login = "login", Id = "id", Email = "email" };
        _userRepoMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken _) => new List<User>(){user});
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateUser_SuccessPath_UpdatesUser()
    {
        // Arrange
        var user = new User()
        {
            Id = "id",
            Email = "email",
            Login = "login"
        };
        
        // Act
        await _service.UpdateAsync(user, CancellationToken.None);
        
        // Assert
        _userRepoMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.Login == user.Login && u.Email == user.Email),
            It.IsAny<CancellationToken>()));
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteUserById_SuccessPath_DeletesUser()
    {
        // Arrange
        const string id = "id";
        _accountRepoMock.Setup(repo => repo.HasAccountsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => false);
        
        // Act
        await _service.DeleteByIdAsync(id, CancellationToken.None);
        
        // Assert
        _userRepoMock.Verify(repo => repo.DeleteByIdAsync(It.Is<string>(u => u == id), It.IsAny<CancellationToken>()));
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteUserById_HasAccounts_ThrowsValidationException()
    {
        // Arrange
        const string id = "id";
        _accountRepoMock.Setup(repo => repo.HasAccountsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => true);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.DeleteByIdAsync(id, CancellationToken.None));
    }
}