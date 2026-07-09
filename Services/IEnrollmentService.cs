using TmsApi.Dtos;

namespace TmsApi.Services;

public interface IEnrollmentService
{

    
         Task<IEnumerable<EnrollmentResponseDto>> GetAllAsync(
        int courseId,
        CancellationToken ct);
        
    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);


    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct);
}