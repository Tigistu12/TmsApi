using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;          
using TmsApi.Application.DTOs;       
using TmsApi.Application.Interfaces;     
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence.Context;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context , ILogger<StudentService> logger) : IStudentService
{
    public Task<StudentResponseDto?>GetByIdAsync(int id, CancellationToken ct)
    {
        return context.Students.AsNoTracking()
        .Where(s => s.Id == id)
        .Select(s => new StudentResponseDto(
            s.Id,
            s.RegistrationNumber,
            s.Name,
            s.Age,
            s.GPA))
            .FirstOrDefaultAsync();
    }

  public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
    PagedRequest request, 
    CancellationToken ct)
    {
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{request.Search}%") ||
            EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);

        query = request.OrderBy switch
        {
            "RegistrationNumber" => request.Descending
            ? query.OrderByDescending(s => s.RegistrationNumber) 
            : query.OrderBy(s => s.RegistrationNumber),

            "Age" => request.Descending
            ? query.OrderByDescending(s => s.Age)
            : query.OrderBy(s => s.Age),

            "GPA" => request.Descending
            ? query.OrderByDescending(s => s.GPA)
            : query.OrderBy(s => s.GPA),

            _ => request.Descending
            ? query.OrderByDescending(s => s.Name)
            : query.OrderBy(s => s.Name)
        };

        var items = await query
        .Skip((request.Page -1) * request.PageSize)
        .Take(request.PageSize)
        .Select(s => new StudentResponseDto(
            s.Id,
            s.RegistrationNumber,
            s.Name,
            s.Age,
            s.GPA))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto>
        {
           Items = items,
           TotalCount = totalCount,
           Page = request.Page,
           PageSize = request.PageSize 
        };
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            Name = request.Name,
            Age = request.Age,
            GPA = request.GPA
        };

        context.Students.Add(student);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created Student {StudentId} ({RegistrationNumber})",
            student.Id,
            student.RegistrationNumber);

        return ( await GetByIdAsync(student.Id, ct))!;
    }


    public async Task<StudentResponseDto?> UpdateAsync(
        int id,
        UpdateStudentRequest request,
        CancellationToken ct)
    {
        var student = await context.Students
        .FirstOrDefaultAsync(s => s.Id == id, ct);

        if(student is null)
        return null;

        student.RegistrationNumber = request.RegistrationNumber;
        student.Name = request.Name;
        student.Age = request.Age;
        student.GPA = request.GPA;

        await context.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }
    
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await context.Students
        .FirstOrDefaultAsync(s => s.Id == id, ct);

        if(student is null)
        return false;

        context.Students.Remove(student);

        await context.SaveChangesAsync(ct);

        return true;
    }

    public Task<bool> RegistrationExistsAsync(string registrationNumber, CancellationToken ct)
    {
        return context.Students.AsNoTracking()
        .AnyAsync(s => s.RegistrationNumber == registrationNumber, ct);
    }
}