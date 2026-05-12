using System.Diagnostics.CodeAnalysis;
using Foundation.Api;
using Foundation.Api.Middleware;
using Foundation.Application;
using Foundation.Infrastructure;
using Serilog;

var port = Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } configuredPort ? configuredPort : "8080";

if (args.Contains("--health-check"))
    return await HealthCheckProbe.RunAsync(port);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.UseUrls($"http://+:{port}");

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        var seqUrl = context.Configuration["Seq:Url"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
            configuration.WriteTo.Seq(seqUrl);
    });

    builder.Services.AddControllers();
    builder.Services.AddOpenApiDocument(settings => settings.Title = "LocalStack Web API");
    builder.Services.AddFoundationApplication();
    builder.Services.AddFoundationInfrastructure();

    var app = builder.Build();

    app.UseMiddleware<CorrelationMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseOpenApi();
        app.UseSwaggerUi();
    }

    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapControllers();
    app.MapFoundationStreaming();
    app.MapFallbackToFile("index.html");

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Application entry point. Exposed as a partial class so that integration tests
/// can host the application with <c>WebApplicationFactory</c>.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
public partial class Program;
