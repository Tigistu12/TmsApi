using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Enrollments.Queries;

public record GetCoursesQuery : IRequest<List<CourseDto>>;