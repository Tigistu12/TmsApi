using MediatR;

namespace TmsApi.Application.Transcripts.Queries;

public record SearchCoursesQuery(string? Term)
    : IRequest<IEnumerable<object>>;