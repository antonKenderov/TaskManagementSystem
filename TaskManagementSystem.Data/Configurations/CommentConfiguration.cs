
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Domain;

namespace TaskManagementSystem.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TaskItemId)
                    .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnName("DateAdded");

            builder.Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(3000);

            builder.Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(c => c.ReminderDate)
                .IsRequired(false);

            builder.Property(t => t.ModifiedAt)
                .IsRequired(false);

            builder.Property(t => t.ModifiedBy)
                .IsRequired(false)
                .HasMaxLength(100);
        }
    }
}
