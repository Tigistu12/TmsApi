using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record UpdateStudentRequest
{
     [Required, RegularExpression(@"^\d{4}-\d{4}$")]
     public required string RegistrationNumber { get; init; }

     [Required, MinLength(2), MaxLength(50)]
    public required string Name { get; init; }

    [Range(18, 60)]
    public int Age { get; set; } 

    [Range(typeof(decimal), "2.0", "4.0")]   
    public decimal GPA { get; set; }
}