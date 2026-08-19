using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;


public interface ITmsDbContext
{
    public DbSet<Student> Students { get; }
    public DbSet<Course> Courses { get; }
    public DbSet<Enrollment> Enrollments { get; }
    public DbSet<Certificate> Certificates { get; }


    public Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default);
}