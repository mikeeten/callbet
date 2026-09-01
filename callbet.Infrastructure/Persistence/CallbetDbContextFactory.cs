using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace callbet.Infrastructure.Persistence
{
    public class CallbetDbContextFactory : IDesignTimeDbContextFactory<CallbetDbContext>
    {
        public CallbetDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CallbetDbContext>();

            // Remember to replace "yourpassword" with your actual database password.
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=callbetdb;Username=postgres;Password=mic123");

            return new CallbetDbContext(optionsBuilder.Options);
        }
    }
}