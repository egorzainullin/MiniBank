using Microsoft.Extensions.DependencyInjection;
using MiniBank.Core.Currency;
using MiniBank.Core.DateTimeProvider;
using MiniBank.Core.Domains.Accounts.Services;
using MiniBank.Core.Domains.BankTransfer.Services;
using MiniBank.Core.Domains.Users.Services;

namespace MiniBank.Core
{
    public static class Bootstraps
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<ICurrencyConverter, CurrencyConverter>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider.DateTimeProvider>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IBankTransferService, BankTransferService>();
            return services;
        }
    }
}