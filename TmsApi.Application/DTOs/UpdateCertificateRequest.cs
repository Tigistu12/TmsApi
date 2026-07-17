using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.DTOs;


public record UpdateCertificateRequest
{
    [Required, RegularExpression(@"^[A-Z]{4}-\d{3}$",
ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
public required string SerialNumber {get; init;}

[Required, MaxLength(200)]
public required string Title {get;init;}

[Required, Range(1, 200)]
  public required int StudentId {get; init;}
    public required int CourseId {get; init;}

}