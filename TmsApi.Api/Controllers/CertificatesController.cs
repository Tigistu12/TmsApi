using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.DTOs;         
using TmsApi.Application.Interfaces;   

namespace TmsApi.Api.Controllers;  

[ApiController]
[Route("api/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]

public class CertificatesController(ICertificateService certificateService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CertificateResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List certificates with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS certificates. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCertificates([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await certificateService.GetCertificatesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCertificateById))]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a certificate by ID")]
    [EndpointDescription("Returns certificate details with HATEOAS links. Returns 404 if the certificate does not exist.")]
    public async Task<IActionResult> GetCertificateById(int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(id, ct);
        if (certificate is null)
        return NotFound();

        var self = linkGenerator.GetPathByName(HttpContext, nameof(GetCertificateById), new { id = certificate.Id })!;

        var links = new List<LinkDto>
        {
            new (self, "self", "GET"),
            new (self, "update", "PUT"),
            new (self, "delete", "DELETE")
        };
        
        var detail = new CertificateDetailDto
{
    Id = certificate.Id,
    SerialNumber = certificate.SerialNumber,
    Title = certificate.Title,
    IssuedAt = DateTime.UtcNow,
    StudentId = certificate.StudentId,
    CourseId = certificate.CourseId,
    Links = links
};

return Ok(detail);

    }

    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new certificate")]
    [EndpointDescription("Creates a certificate with a unique serial number. Returns409 if the certificate serial number already exists.")]
    public async Task<IActionResult> CreateCertificate(CreateCertificateRequest request, CancellationToken ct)
    {
        if (await certificateService.SerialNumberExistsAsync(request.SerialNumber, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Certificate serial number already exists",
                Detail = $"A Certificate with serial number '{request.SerialNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await certificateService.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetCertificateById),
            new { id = result.Id },
            result);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a certificate")]
    [EndpointDescription("Updates a certificate with a unique serial number. Returns404 if the certificate does not exist.")]

    public async Task<IActionResult> UpdateCertificate(int id, UpdateCertificateRequest request, CancellationToken ct)
    {
        var certificate = await certificateService.UpdateAsync(id, request, ct);

        if (certificate is null)
        return NotFound();

        return Ok(certificate);
    }


    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a certificate")]
    [EndpointDescription("Deletes a certificate with a unique serial number. Returns404 if the certificate does not exist.")]

    public async Task<IActionResult> DeleteCertificate(int id, CancellationToken ct)
    {
        var deleted = await certificateService.DeleteAsync(id, ct);

        if (!deleted)
        return NotFound();

        return NoContent();
    }
}

