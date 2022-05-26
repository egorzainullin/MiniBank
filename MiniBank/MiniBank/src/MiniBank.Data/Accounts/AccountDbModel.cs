using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBank.Data.Users;

namespace MiniBank.Data.Accounts;

public class AccountDbModel
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public double Amount { get; set; }
    
    public string Currency { get; set; }

    public bool IsOpened { get; set; }

    public DateTime DateWhenOpened { get; set; }

    public DateTime DateWhenClosed { get; set; }
    
    internal class Map : IEntityTypeConfiguration<AccountDbModel>
    {
        public void Configure(EntityTypeBuilder<AccountDbModel> builder)
        {
            builder.ToTable("account");
        }
    }
}