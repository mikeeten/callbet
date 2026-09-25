using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
    {
        public void Configure(EntityTypeBuilder<Neighborhood> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id).ValueGeneratedOnAdd();
            builder.Property(n => n.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(n => n.SubCity)
                   .WithMany(s => s.Neighborhoods)
                   .HasForeignKey(n => n.SubCityId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(n => n.Addresses)
                   .WithOne(a => a.Neighborhood)
                   .HasForeignKey(a => a.NeighborhoodId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
