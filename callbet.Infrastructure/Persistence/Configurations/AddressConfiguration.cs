using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Country).HasMaxLength(100);
            builder.Property(a => a.City).HasMaxLength(100);
            builder.Property(a => a.Street).HasMaxLength(255);
            builder.Property(a => a.Label).HasMaxLength(100);
            builder.Property(a => a.Landmark).HasMaxLength(255);
            builder.Property(a => a.PrimaryPhone).HasMaxLength(20);

            builder.Property(a => a.Latitude).HasColumnType("decimal(10, 8)");
            builder.Property(a => a.Longitude).HasColumnType("decimal(11, 8)");

            builder.HasOne(a => a.Neighborhood)
                   .WithMany(n => n.Addresses)
                   .HasForeignKey(a => a.NeighborhoodId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.User)
                   .WithMany(u => u.Addresses)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}