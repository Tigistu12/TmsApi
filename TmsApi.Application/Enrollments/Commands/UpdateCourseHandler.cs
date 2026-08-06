using MediatR;

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
namespace TmsApi.Application.Enrollments.Commands;

using TmsApi.Domain.Entities;
public class UpdateCourseCommandHandler 
    : IRequestHandler<UpdateCourseCommand, bool>
{

    private readonly ITmsDbContext _context;
    private readonly ICachedCourseService _cachedCourseService;


    public UpdateCourseCommandHandler(
        ITmsDbContext context,
        ICachedCourseService cachedCourseService)
    {
        _context = context;
        _cachedCourseService = cachedCourseService;
    }



    public async Task<bool> Handle(
        UpdateCourseCommand command,
        CancellationToken ct)
    {

        var course = _context.Courses
            .FirstOrDefault(c => c.Id == command.Id);


        if(course is null)
        {
            return false;
        }



        course.Code = command.Code;
        course.Title = command.Title;
        course.MaxCapacity = command.MaxCapacity;



        await _context.SaveChangesAsync(ct);



        // Important:
        // remove old cached data
        await _cachedCourseService
            .InvalidateCourseCacheAsync(ct);



        return true;
    }
}