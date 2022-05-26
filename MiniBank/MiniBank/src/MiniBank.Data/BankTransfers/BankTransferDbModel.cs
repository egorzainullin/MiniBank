using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBank.Data.Accounts;

namespace MiniBank.Data.BankTransfers;

public class BankTransferDbModel
{
    public string Id { get; set; }

    public double Amount { get; set; }

    public string Currency { get; set; }

    public string FromAccountId { get; set; }

    public string ToAccountId { get; set; }

    internal class Map : IEntityTypeConfiguration<BankTransferDbModel>
    {
        public void Configure(EntityTypeBuilder<BankTransferDbModel> builder)
        {
            builder.ToTable("transfer");
        }
    }
}