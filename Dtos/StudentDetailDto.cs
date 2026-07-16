using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TmsApi.Dtos
{
    public record StudentDetailDto
    {
        public required int Id { get; init; }
        public required string RegistrationNumber { get; init; }
        public required string Name { get; init; }
        public required int Age { get; init; }
        public required decimal GPA { get; init; }
        public required IReadOnlyList<LinkDto> Links { get; init; }
    }
}