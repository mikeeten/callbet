using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder)
        {
            builder.HasKey(sc => sc.Id);

            builder.Property(sc => sc.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(sc => sc.Name).IsUnique();

            builder.HasOne(sc => sc.ParentCategory)
                .WithMany(pc => pc.SubCategories)
                .HasForeignKey(sc => sc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sc => sc.Services)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.CategoryId);
        }
    }
}