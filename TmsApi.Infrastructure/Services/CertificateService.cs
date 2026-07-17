
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;          
using TmsApi.Application.DTOs;       
using TmsApi.Application.Interfaces;     
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CertificateService(TmsDbContext context, ILogger<CertificateService> logger) : ICertificateService
{
    public Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return context.Certificates.AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CertificateResponseDto(
            c.Id,
            c.SerialNumber,
            c.Title,
            c.IssuedAt,
            c.StudentId,
            c.CourseId))
        .FirstOrDefaultAsync(ct);
    }

    public async Task<CertificateResponseDto> CreateAsync(CreateCertificateRequest request, CancellationToken ct)
    {
        var certificate = new Certificate
        {
            SerialNumber = request.SerialNumber,
            Title = request.Title,
            IssuedAt = DateTime.UtcNow,
            StudentId = request.StudentId,
            CourseId = request.CourseId
        };

        context.Certificates.Add(certificate);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created Certificate {CertificateId} ({SerialNumber})",
            certificate.Id,
            certificate.SerialNumber);

        return (await GetByIdAsync(certificate.Id, ct))!;
    }

    public async Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(
        PagedRequest request, CancellationToken ct)
    {
        IQueryable<Certificate> query = context.Certificates.AsNoTracking();

        // apply search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.SerialNumber, $"%{request.Search}%") ||
                EF.Functions.ILike(c.Title, $"%{request.Search}%"));
        }

        // count before paging
        var totalCount = await query.CountAsync(ct);

        // apply sorting
        query = request.OrderBy switch
        {
            "SerialNumber" => request.Descending
                ? query.OrderByDescending(c => c.SerialNumber)
                : query.OrderBy(c => c.SerialNumber),

            "Title" => request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title),

            _ => request.Descending
                ? query.OrderByDescending(c => c.IssuedAt)
                : query.OrderBy(c => c.IssuedAt)
        };

        return new PagedResponse<CertificateResponseDto>
        {
            Items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.Title,
                c.IssuedAt,
                c.StudentId,
                c.CourseId))
            .ToListAsync(ct),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }


    public async Task<CertificateResponseDto?> UpdateAsync(int id, UpdateCertificateRequest request, CancellationToken ct)
    {
        var certificate = await context.Certificates.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (certificate is null)
        return null;

        certificate.SerialNumber = request.SerialNumber;
        certificate.Title = request.Title;
        certificate.StudentId = request.StudentId;
        certificate.CourseId = request.CourseId;

        await context.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var certificate = await context.Certificates.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (certificate is null)
        return false;

        context.Certificates.Remove(certificate);

        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct)
    {
        return await context.Certificates
        .AsNoTracking()
        .AnyAsync(c => c.SerialNumber == serialNumber, ct); 
    }
}

