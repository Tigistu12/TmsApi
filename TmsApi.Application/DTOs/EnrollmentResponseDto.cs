namespace TmsApi.Application.DTOs;

public record EnrollmentResponseDto(
int Id,
int CourseId,
int StudentId,
string Status,
DateTime EnrolledAt);