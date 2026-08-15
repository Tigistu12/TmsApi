using MediatR;

namespace TmsApi.Application.Grades.Commands;

public record GradeResponseDto(string Id, bool Success);

public record PostGradeCommand(int StudentId, int CourseId, double Score) : IRequest<GradeResponseDto>;

public class PostGradeCommandHandler : IRequestHandler<PostGradeCommand, GradeResponseDto>
{
    public async Task<GradeResponseDto> Handle(PostGradeCommand request, CancellationToken ct)
    {
        // TODO: Persist grade record to database using your DbContext
        Console.WriteLine($"[TMS Backend] Processing grade submission for Student: {request.StudentId}");
    
            await Task.Delay(100, ct); // Simulated async work

        // Return expected response object matching Angular GradeService interface
        return new GradeResponseDto(Guid.NewGuid().ToString(), true);
    }
}