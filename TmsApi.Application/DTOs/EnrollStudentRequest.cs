using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.DTOs;

public record EnrollStudentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "StudnetId must be a positive integer.")]
    public required int StudentId {get; init;}
}