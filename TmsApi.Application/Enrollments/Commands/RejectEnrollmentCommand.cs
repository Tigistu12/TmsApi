using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces; 
namespace TmsApi.Application.Enrollments.Commands;
using TmsApi.Domain.Entities;

// 1. Command Definition
public record RejectEnrollmentCommand(int Id) : IRequest<Enrollment?>;

// 2. MediatR Command Handler (using Primary Constructor)

public class RejectEnrollmentCommandHandler
    : IRequestHandler<RejectEnrollmentCommand, Enrollment?>
{
    private readonly ITmsDbContext _context;

    public RejectEnrollmentCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> Handle(
        RejectEnrollmentCommand request,
        CancellationToken ct)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(
                e => e.Id == request.Id,
                ct);

        if (enrollment is null)
            return null;

        // Change status
        enrollment.Status = "Rejected";

        await _context.SaveChangesAsync(ct);

        return enrollment;
    }
}