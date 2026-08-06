namespace TmsApi.Application.Enrollments.Queries;

using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.DTOs;
using TmsApi.Application.Enrollments.Queries;
public class GetCoursesQueryHandler
    : IRequestHandler<GetCoursesQuery, List<CourseDto>>
{

    private readonly ICachedCourseService cachedService;


    public GetCoursesQueryHandler(
        ICachedCourseService cachedService)
    {
        this.cachedService = cachedService;
    }



    public async Task<List<CourseDto>> Handle(
        GetCoursesQuery query,
        CancellationToken ct)
    {
        return await cachedService.GetAllCoursesAsync(ct);
    }
}