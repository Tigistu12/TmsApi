using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TmsApi.Tests;

public class CoursesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoursesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCourses_ReturnsOkAndPagedJson()
    {
        // Act
        var response = await _client.GetAsync("/api/v2.0/courses?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var page = await response.Content.ReadFromJsonAsync<PagedCoursesJson>();
        Assert.NotNull(page?.Data);
    }

    [Fact]
    public async Task CreateCourse_InvalidCode_ReturnsValidationError()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v2.0/courses", new
        {
            code = "",
            title = "Intro to TMS Security",
            maxCapacity = 30
        });

        // Assert
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity,
            $"Expected BadRequest or UnprocessableEntity, but received {response.StatusCode}");
    }

    private sealed class PagedCoursesJson
    {
        public List<CourseRowJson> Data { get; set; } = default!;
    }

    private sealed class CourseRowJson
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public int MaxCapacity { get; set; }
        public int EnrollmentCount { get; set; }
    }
}