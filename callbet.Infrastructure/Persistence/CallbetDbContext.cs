using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using callbet.Domain.Entities;

namespace callbet.Infrastructure.Persistence;

public class CallbetDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public CallbetDbContext(DbContextOptions<CallbetDbContext> options) : base(options) { }

    public DbSet<ProfessionalProfile> ProfessionalProfiles => Set<ProfessionalProfile>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<SubCity> SubCities => Set<SubCity>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ProfessionalService> ProfessionalServices => Set<ProfessionalService>();
    public DbSet<AvailabilitySchedule> AvailabilitySchedules => Set<AvailabilitySchedule>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewReply> ReviewReplies => Set<ReviewReply>();
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
    public DbSet<FavoriteProfessional> FavoriteProfessionals => Set<FavoriteProfessional>();
    public DbSet<FavoriteService> FavoriteServices => Set<FavoriteService>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<VerificationRecord> VerificationRecords => Set<VerificationRecord>();
    public DbSet<AdminLog> AdminLogs => Set<AdminLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply configurations from the current assembly (where CallbetDbContext is defined)
        // This assumes you will define your entity configurations (e.g., UserConfiguration.cs)
        // in the same assembly as your DbContext, or a referenced assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CallbetDbContext).Assembly);
    }
    public CallbetDbContext() { } 
}