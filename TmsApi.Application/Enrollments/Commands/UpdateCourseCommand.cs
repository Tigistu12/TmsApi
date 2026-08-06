using MediatR;

namespace TmsApi.Application.Enrollments.Commands;

public record UpdateCourseCommand(
    int Id,
    string Code,
    string Title,
    int MaxCapacity
) : IRequest<bool>;