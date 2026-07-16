using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateCertificateRequest
{
    [Required, RegularExpression(@"^[A-Z]{4}-\d{3}$",
ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    public required string SerialNumber {get; init;}

    public required string Title {get; init;}
    [Required]
    public required int StudentId {get; init;}
    [Required]
    public required int CourseId {get; init;}
   
}