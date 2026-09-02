
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Domain;

namespace TaskManagementSystem.Data.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.RequiredByDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.AssignedToUserId)
                .IsRequired();

            builder.HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.ModifiedAt)
                .IsRequired(false);

            builder.Property(t => t.ModifiedBy)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.HasMany(t => t.Comments)
                .WithOne(c => c.TaskItem)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
