namespace TmsApi.Application.DTOs;

public record EnrollmentResponseDto(
int Id,
int CourseId,
string Title,
int StudentId,
string StudentName,
string Status,
DateTime EnrolledAt);