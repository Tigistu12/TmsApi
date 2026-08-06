using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public record GetAllEnrollmentQuery : IRequest<List<EnrollmentResponseDto>>;

internal class GetAllEnrollmentQueryHandler : IRequestHandler<GetAllEnrollmentQuery, List<EnrollmentResponseDto>>
{
    private readonly IEnrollmentService enrollmentService;

    public GetAllEnrollmentQueryHandler(IEnrollmentService enrollmentService)
    {
        this.enrollmentService = enrollmentService;
    }

    public async Task<List<EnrollmentResponseDto>> Handle(GetAllEnrollmentQuery request, CancellationToken ct)
    {
        return await enrollmentService.GetAllAsync(ct);
    }
}