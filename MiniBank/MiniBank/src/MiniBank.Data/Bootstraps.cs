using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniBank.Core;
using MiniBank.Core.Currency;
using MiniBank.Core.Domains.Accounts.Repositories;
using MiniBank.Core.Domains.BankTransfer.Repositories;
using MiniBank.Core.Domains.Users.Repositories;
using MiniBank.Core.UnitOfWork;
using MiniBank.Data.Accounts.Repositories;
using MiniBank.Data.BankTransfers.Repositories;
using MiniBank.Data.Users.Repositories;

namespace MiniBank.Data
{
    public static class Bootstraps
    
    {
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ICurrencyCourseProvider, CurrencyCourseHttpProvider>(options  =>
            {
                options.BaseAddress = new Uri(configuration["UriToConverter"]);
            });
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IBankTransferRepository, BankTransferRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddDbContext<DatabaseContext>(options => options
                .UseLazyLoadingProxies()
                .UseNpgsql(
                    string.Format(configuration["LogDatabase"], "host.docker.internal")));

            return services;
        }
    }
}