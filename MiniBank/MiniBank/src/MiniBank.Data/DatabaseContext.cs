using System.Diagnostics;
using MiniBank.Data.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using MiniBank.Data.Accounts;
using MiniBank.Data.BankTransfers;

namespace MiniBank.Data;

public class DatabaseContext: DbContext
{
    public DbSet<UserDbModel> Users { get; set; }
    
    public DbSet<AccountDbModel> Accounts { get; set; }
    
    public DbSet<BankTransferDbModel> BankTransfers { get; set; }
    
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.LogTo(Console.WriteLine);
        base.OnConfiguring(optionsBuilder);
    }
}

public class Factory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql("Some connection string")
            .Options;

        return new DatabaseContext(options);
    }
}