using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniBank.Data.Users;

public class UserDbModel
{
    public string Id { get; set; }

    public string Login { get; set; }

    public string Email { get; set; }

    internal class Map : IEntityTypeConfiguration<UserDbModel>
    {
        public void Configure(EntityTypeBuilder<UserDbModel> builder)
        {
            builder.ToTable("user");
        }
    }
}