
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Domain;

namespace TaskManagementSystem.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasData(
                new User { Id = 1, Name = "Anton" },
                new User { Id = 2, Name = "Borislav" },
                new User { Id = 3, Name = "Georgi" },
                new User { Id = 4, Name = "Petur" }
            );
        }
    }
}
