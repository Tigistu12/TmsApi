namespace TmsApi.Application.DTOs;

public record CourseDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount
);