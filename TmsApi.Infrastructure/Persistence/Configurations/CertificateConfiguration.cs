using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities; 
namespace TmsApi.Infrastructure.Persistence.Configurations; 

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.SerialNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.IssuedAt).IsRequired();
        builder.HasOne(c => c.Student).WithMany(s => s.Certificates).HasForeignKey(c => c.StudentId);
        builder.HasOne(c => c.Course).WithMany(c => c.Certificates).HasForeignKey(c => c.CourseId);
        builder.HasIndex(c => c.SerialNumber).IsUnique();
       
    }
}