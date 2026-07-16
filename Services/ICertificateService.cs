using TmsApi.Dtos;
namespace TmsApi.Services;

public interface ICertificateService
{
    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CertificateResponseDto> CreateAsync(CreateCertificateRequest request, CancellationToken ct);
    Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(PagedRequest request, CancellationToken ct);
    Task<CertificateResponseDto?> UpdateAsync(int id, UpdateCertificateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct);
}