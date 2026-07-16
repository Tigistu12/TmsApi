namespace TmsApi.Dtos;

public record CertificateResponseDto(
    int Id,
    string SerialNumber,
    string Title,
    DateTime IssuedAt,
    int StudentId,
    int CourseId
);