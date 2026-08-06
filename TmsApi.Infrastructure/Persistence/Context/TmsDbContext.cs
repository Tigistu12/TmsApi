using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
namespace TmsApi.Infrastructure.Persistence.Context;
public class TmsDbContext (
    DbContextOptions<TmsDbContext> options)
     : DbContext(options),ITmsDbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<Certificate> certificates => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(TmsDbContext).Assembly);

    base.OnModelCreating(modelBuilder);
}

     public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    foreach (var entry in ChangeTracker.Entries<Student>())
    {
        if (entry.State == EntityState.Added ||
            entry.State == EntityState.Modified)
        {
            entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        }
    }

    return await base.SaveChangesAsync(cancellationToken);
}
  
}