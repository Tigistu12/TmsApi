using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Transcripts.Queries;

public class SearchCoursesQueryHandler
    : IRequestHandler<SearchCoursesQuery, IEnumerable<object>>
{
    private readonly ITmsDbContext _context;

    public SearchCoursesQueryHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<object>> Handle(
        SearchCoursesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Courses
            .Where(c => request.Term == null ||
                        c.Title.Contains(request.Term))
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code
            })
            .ToListAsync(cancellationToken);
    }
}