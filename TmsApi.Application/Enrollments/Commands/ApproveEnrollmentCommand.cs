using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Commands;

public record ApproveEnrollmentCommand(int EnrollmentId): IRequest<bool>;

internal class ApproveEnrollmentCommandHandler(ITmsDbContext context)
    : IRequestHandler<ApproveEnrollmentCommand, bool>
{
    public async Task<bool> Handle(ApproveEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == request.EnrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return false;
        }

        enrollment.Status = "Approved";
        context.Enrollments.Update(enrollment);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}