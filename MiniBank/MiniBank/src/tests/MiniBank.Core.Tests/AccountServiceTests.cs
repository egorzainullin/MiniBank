using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MiniBank.Core.Currency;
using MiniBank.Core.DateTimeProvider;
using MiniBank.Core.Domains.Accounts;
using MiniBank.Core.Domains.Accounts.Repositories;
using MiniBank.Core.Domains.Accounts.Services;
using MiniBank.Core.Domains.BankTransfer.Repositories;
using MiniBank.Core.Domains.Users;
using MiniBank.Core.Domains.Users.Repositories;
using MiniBank.Core.Exceptions;
using MiniBank.Core.UnitOfWork;
using Moq;
using Xunit;

namespace MiniBank.Core.Tests;

public class AccountServiceTests
{
    private readonly IAccountService _service;

    private readonly Mock<IAccountRepository> _accountMock;

    private readonly Mock<IUserRepository> _userRepositoryMock;

    private readonly Mock<IBankTransferRepository> _bankTransferRepositoryMock;

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly Mock<IDateTimeProvider> _timeProviderMock;

    private readonly Mock<ICurrencyCourseProvider> _courseProviderMock;

    private CurrencyConverter _currencyConverter;

    public AccountServiceTests()
    {
        _accountMock = new Mock<IAccountRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _bankTransferRepositoryMock = new Mock<IBankTransferRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _timeProviderMock = new Mock<IDateTimeProvider>();
        _courseProviderMock = new Mock<ICurrencyCourseProvider>();

        _currencyConverter = new CurrencyConverter(_courseProviderMock.Object);
        _service = new AccountService(_accountMock.Object, _userRepositoryMock.Object,
            _bankTransferRepositoryMock.Object,
            _currencyConverter, _unitOfWorkMock.Object, _timeProviderMock.Object);
    }

    [Fact]
    public async Task GetAccountsByUserId_SuccessPath_ReturnsAccounts()
    {
        // Arrange
        const string userId = "userId";
        var account1 = new Account
        {
            Id = "id1",
            UserId = userId,
            Amount = 10,
            Currency = Currency.Currency.Eur,
            DateWhenClosed = new DateTime(2022, 2, 22),
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = false
        };
        var account2 =
            new Account
            {
                Id = "id2",
                UserId = userId,
                Amount = 20,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };
        var accounts = new List<Account>
        {
            account1,
            account2
        };
        _accountMock.Setup(repo =>
                repo.GetAccountsByUserIdAsync(It.Is<string>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => accounts);
        
        // Act
        var returnedAccounts = await _service.GetAccountsByUserIdAsync(userId, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, returnedAccounts.Count);
        Assert.Contains<Account>(returnedAccounts, account => account.Id == account1.Id);
        Assert.Contains<Account>(returnedAccounts, account => account.Id == account2.Id);
    }

    [Fact]
    public async Task CreateAccount_SuccessPath_ShouldCreateNewAccount()
    {
        // Arrange
        const string userId = "userId";
        var user = new User()
        {
            Id = userId
        };
        const double amount = 50.0;
        const string currency = "EUR";
        var date = new DateTime(2022, 2, 22);

        _timeProviderMock.Setup(time => time.UtcNow())
            .Returns(() => date);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(It.Is<string>(s => s == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => user);

        // Act
        await _service.CreateAsync(userId, currency, amount, CancellationToken.None);
        
        // Assert
        _accountMock.Verify(x =>
            x.CreateAsync(It.Is<string>(it => it == userId),
                It.Is<string>(curr => curr == currency),
                It.Is<double>(a => Math.Abs(a - amount) < 0.01),
                It.Is<DateTime>(d => d == date), It.IsAny<CancellationToken>()));
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task CreateAccount_WithCurrencyThatDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        const string userId = "userId";
        var user = new User()
        {
            Id = userId
        };
        const double amount = 50.0;
        const string currency = "EUS";
        var date = new DateTime(2022, 2, 22);
        _timeProviderMock.Setup(time => time.UtcNow())
            .Returns(() => date);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(It.Is<string>(s => s == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => user);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(userId, currency, amount, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAccount_WithUserThatDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        const string userId = "userId";
        const double amount = 50.0;
        const string currency = "EUR";
        var date = new DateTime(2022, 2, 22);
        _timeProviderMock.Setup(time => time.UtcNow())
            .Returns(() => date);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(It.Is<string>(s => s == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => null);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(userId, currency, amount, CancellationToken.None));
    }

    [Fact]
    public async Task CloseAccount_SuccessPath_ClosesAccount()
    {
        // Arrange
        const string accountId = "id";
        const string userId = "userId";
        const double amount = 0.0;
        var dateOpened = new DateTime(2021, 2, 21);
        var dateClosed = new DateTime(2022, 2, 22);
        var account =
            new Account
            {
                Id = accountId,
                UserId = userId,
                Amount = amount,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = dateOpened,
                IsOpened = true
            };
        _timeProviderMock.Setup(time => time.UtcNow())
            .Returns(() => dateClosed);
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => account);
        
        // Act
        await _service.CloseAccountAsync(accountId, CancellationToken.None);
        
        // Assert
        _accountMock.Verify(repo => repo.CloseAccountAsync(It.Is<string>(i => i == accountId),
            It.Is<DateTime>(time => time == dateClosed), It.IsAny<CancellationToken>()));
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task CloseAccount_WithNotZeroAmount_ThrowsValidationException()
    {
        // Arrange
        const string accountId = "id";
        const string userId = "userId";
        const double amount = 50.0;
        var dateOpened = new DateTime(2021, 2, 21);
        var dateClosed = new DateTime(2022, 2, 22);
        var account =
            new Account
            {
                Id = accountId,
                UserId = userId,
                Amount = amount,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = dateOpened,
                IsOpened = true
            };
        _timeProviderMock.Setup(time => time.UtcNow())
            .Returns(() => dateClosed);
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => account);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CloseAccountAsync(accountId, CancellationToken.None));
    }

    [Fact]
    public async Task CloseAccount_WithAlreadyClosedAccount_ThrowsValidationException()
    {
        // Arrange
        const string accountId = "id";
        const string userId = "userId";
        const double amount = 50.0;
        var dateOpened = new DateTime(2021, 2, 21);
        var dateClosed = new DateTime(2022, 2, 22);
        var account =
            new Account
            {
                Id = accountId,
                UserId = userId,
                Amount = amount,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = dateOpened,
                DateWhenClosed = dateClosed,
                IsOpened = false
            };
        _timeProviderMock.Setup(time => time.UtcNow())
            .Returns(() => dateClosed);
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => account);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CloseAccountAsync(accountId, CancellationToken.None));
    }

    [Fact]
    public async Task CloseAccount_ThatDoesNotExist_ThrowsObjectNotFoundException()
    {
        // Arrange
        _accountMock.Setup(repo =>
                repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, CancellationToken _) => null);
        
        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotFoundException>(() =>
            _service.CloseAccountAsync("some id", CancellationToken.None));
    }

    private static double GetRubleCourse(Currency.Currency currency)
    {
        return currency switch
        {
            Currency.Currency.Eur => 200,
            Currency.Currency.Usd => 100,
            Currency.Currency.Rub => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null)
        };
    }

    [Fact]
    public async Task CalculateCommission_FromDifferentAccounts_ReturnsTwoPercent()
    {
        // Arrange
        const string userId1 = "userId1";
        const string userId2 = "userId2";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId1,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId2,
                Amount = 200,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };

        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        _currencyConverter = new CurrencyConverter(_courseProviderMock.Object);

        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);
        
        // Act
        var commission = await _service.CalculateCommissionAsync(50, id1, id2, CancellationToken.None);
        
        // Assert
        Assert.Equal(1, commission);
    }

    [Fact]
    public async Task CalculateCommission_WithFromAccountThatDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        const string id1 = "id1";
        const string id2 = "id2";
        const string userId = "userId";

        var account = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenClosed = new DateTime(2022, 2, 22),
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };

        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account : null);

        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotFoundException>(() =>
            _service.CalculateCommissionAsync(50, id1, id2, CancellationToken.None));
    }

    [Fact]
    public async Task CalculateCommission_WithToAccountThatDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        const string id1 = "id1";
        const string id2 = "id2";
        const string userId = "userId";

        var account = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenClosed = new DateTime(2022, 2, 22),
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };

        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id2 ? account : null);

        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotFoundException>(() =>
            _service.CalculateCommissionAsync(50, id1, id2, CancellationToken.None));
    }

    [Fact]
    public async Task CalculateCommission_WithSameUser_ReturnsZero()
    {
        // Arrange
        const string userId = "userId";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenClosed = new DateTime(2022, 2, 22),
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId,
                Amount = 200,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);

        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        
        // Act
        var commission = await _service.CalculateCommissionAsync(50, id1, id2, CancellationToken.None);
        
        // Assert
        Assert.Equal(0, commission);
    }

    [Fact]
    public async Task Transfer_DifferentUsers_ShouldTransfer()
    {
        // Arrange
        const string userId1 = "userId1";
        const string userId2 = "userId2";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId1,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId2,
                Amount = 200,
                Currency = Currency.Currency.Rub,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);
        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        
        // Act
        await _service.TransferAsync(50, id1, id2, CancellationToken.None);

        // Assert
        _accountMock.Verify(repo =>
            repo.UpdateAmountAsync(id1, 50, CancellationToken.None));
        _accountMock.Verify(repo =>
            repo.UpdateAmountAsync(id2, 10000, CancellationToken.None));
        _bankTransferRepositoryMock.Verify(repo =>
            repo.CreateAsync(50, "EUR", id1, id2, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Transfer_SameUsers_ShouldTransfer()
    {
        // Arrange
        const string userId = "userId";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId,
                Amount = 200,
                Currency = Currency.Currency.Usd,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);
        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        
        // Act
        await _service.TransferAsync(50, id1, id2, CancellationToken.None);
        
        // Assert
        _accountMock.Verify(repo =>
            repo.UpdateAmountAsync(id1, 50, It.IsAny<CancellationToken>()));
        _accountMock.Verify(repo =>
            repo.UpdateAmountAsync(id2, 300, It.IsAny<CancellationToken>()));
        _bankTransferRepositoryMock.Verify(repo =>
            repo.CreateAsync(50, "EUR", id1, id2, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Transfer_WithFromAccountThatDoesNotExist_ThrowsObjectNotFoundException()
    {
        // Arrange
        const string id1 = "id1";
        const string id2 = "id2";
        const string userId = "userId";

        var account = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenClosed = new DateTime(2022, 2, 22),
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account : null);
        
        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotFoundException>(() =>
            _service.TransferAsync(50, id1, id2, CancellationToken.None));
    }

    [Fact]
    public async Task Transfer_WithToAccountThatDoesNotExist_ThrowsObjectNotFoundException()
    {
        // Arrange
        const string id1 = "id1";
        const string id2 = "id2";
        const string userId = "userId";

        var account = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenClosed = new DateTime(2022, 2, 22),
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id2 ? account : null);
        
        // Act & Assert
        await Assert.ThrowsAsync<ObjectNotFoundException>(() =>
            _service.TransferAsync(50, id1, id2, CancellationToken.None));
    }

    [Fact]
    public async Task Transfer_WithNotOpenedFromAccount_ThrowsValidationException()
    {
        // Arrange
        const string userId = "userId";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId,
                Amount = 200,
                Currency = Currency.Currency.Usd,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = false
            };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);
        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.TransferAsync(50, id1, id2, CancellationToken.None));
    }

    [Fact]
    public async Task Transfer_WithNotOpenedToAccount_ThrowsValidationException()
    {
        // Arrange
        const string userId = "userId";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = false
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId,
                Amount = 200,
                Currency = Currency.Currency.Usd,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);
        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.TransferAsync(50, id1, id2, CancellationToken.None));
    }

    [Fact]
    public async Task Transfer_WithAmountBiggerOnTheAccount_ThrowsValidationException()
    {
        // Arrange
        const string userId = "userId";
        const string id1 = "id1";
        const string id2 = "id2";

        var account1 = new Account
        {
            Id = id1,
            UserId = userId,
            Amount = 100,
            Currency = Currency.Currency.Eur,
            DateWhenOpened = new DateTime(2022, 2, 21),
            IsOpened = true
        };
        var account2 =
            new Account
            {
                Id = id2,
                UserId = userId,
                Amount = 200,
                Currency = Currency.Currency.Usd,
                DateWhenOpened = new DateTime(2021, 2, 21),
                IsOpened = true
            };
        _accountMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => id == id1 ? account1 : account2);
        _courseProviderMock.Setup(provider => provider.GetRubleCourseAsync(It.IsAny<Currency.Currency>()))
            .ReturnsAsync((Currency.Currency c) => GetRubleCourse(c));
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.TransferAsync(500, "id1", "id2", CancellationToken.None));
    }
}