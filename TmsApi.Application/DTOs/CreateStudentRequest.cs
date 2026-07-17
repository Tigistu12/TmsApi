using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record CreateStudentRequest
{
    [Required, RegularExpression(@"^\d{4}-\d{4}$",
    ErrorMessage = "Registration number must be in the format of YYYY-0001")]
    public required string RegistrationNumber {get; init;}

    [Required, MaxLength(50), MinLength(2), RegularExpression(@"^[A-Za-z ]+$",
    ErrorMessage = "Name must contain only letters.")]
    public required string Name {get; init;}

    [Required, Range(18, 60)]
    public int Age {get; init;}

    [Range(typeof(decimal), "2.0", "4.0")]
    public decimal GPA {get; init;} 
}