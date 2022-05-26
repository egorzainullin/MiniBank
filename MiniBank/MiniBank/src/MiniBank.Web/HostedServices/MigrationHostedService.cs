using Microsoft.EntityFrameworkCore;
using MiniBank.Data;

namespace MiniBank.Web.HostedServices
{
    public class MigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public MigrationHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DatabaseContext>();

                if (context == null)
                {
                    throw new Exception($"{nameof(DatabaseContext)} not registered");
                }
                
                context.Database.Migrate();
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}