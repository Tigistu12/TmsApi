// exercise 2: the memory leak(captive dependencies)

using TmsApi.Services;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<IEnrollmentService>();

                Console.WriteLine("Batch processing completed");
    }
}

