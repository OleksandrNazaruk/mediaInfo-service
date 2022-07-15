using _MediaInfoService;
using _MediaInfoService.Extensions;
using _MediaInfoService.HealthCheck;
using _MediaInfoService.InputFormatters;
using _MediaInfoService.Models;
using FFmpeg.AutoGen;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;

Program.PrintProductVersion();


var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions()
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    });

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = String.Format("{0} ({1})", Program.ServiceName, Program.InstanceName);
});

builder.Host.UseSystemd();



#region ConfigureAppConfiguration
#region WorkingDirectory
var workingDirectory = builder.Environment.ContentRootPath;
if (Environment.OSVersion.Platform == PlatformID.Win32NT)
{
    workingDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FreeHand", builder.Environment.ApplicationName);
}
else if (Environment.OSVersion.Platform == PlatformID.Unix)
{
    workingDirectory = System.IO.Path.Combine($"/opt/", builder.Environment.ApplicationName, "etc", builder.Environment.ApplicationName);
}
System.IO.Directory.CreateDirectory(workingDirectory);

builder.Configuration.SetBasePath(workingDirectory);

// add workingDirectory service configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            {"WorkingDirectory", workingDirectory}
                        });
#endregion

Console.WriteLine($"$Env:EnvironmentName={builder.Environment.EnvironmentName}");
Console.WriteLine($"$Env:ApplicationName={builder.Environment.ApplicationName}");
Console.WriteLine($"$Env:ContentRootPath={builder.Environment.ContentRootPath}");
Console.WriteLine($"WorkingDirectory={workingDirectory}");

builder.Configuration.AddJsonFile($"{builder.Environment.ApplicationName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddIniFile($"{builder.Environment.ApplicationName}.conf", optional: true, reloadOnChange: true);
builder.Configuration.AddCommandLine(args);
builder.Configuration.AddEnvironmentVariables();
#endregion

#region ConfigureLogging
builder.Logging.AddConfiguration((IConfiguration)builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddEventLog();
#endregion

#region ConfigureWebHostDefaults

var webBuilder = builder.WebHost;
webBuilder.UseKestrel((context, serverOptions) =>
{
    serverOptions.Configure((IConfiguration)context.Configuration.GetSection("Kestrel"))
        .Endpoint("HTTPS", listenOptions =>
        {
            listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
        });
});
#endregion

#region ConfigureServices
// Configuration
builder.Services.Configure<GlobalSettings>(builder.Configuration);

//
builder.Services.AddHttpClient();

// add services (BackgroundTaskQueue, FFmpegLogger)
builder.Services.AddServices();

// cors
builder.Services.ConfigureCors();

// HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck<BaseHealthCheck>("MediaInfo.Service.Health");


// add controllers
builder.Services
    .AddControllers(options => options.InputFormatters.Insert(options.InputFormatters.Count, new TextPlainInputFormatter()))
    .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);
   

// Register the Swagger generator, defining 1 or more Swagger documents
builder.Services.ConfigureSwagger();

#endregion


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// configure UI StaticFiles
app.ConfigureStaticFiles(
    builder.Configuration.GetValue<string>("WorkingDirectory"));

// Enable middleware to serve generated Swagger as a JSON endpoint.
//app.UseSwaggerAuthorized();
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediaInfo Service REST API V1");
});

app.UseRouting();

app.UseCors("CorsPolicy");

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions()
{
    AllowCachingResponses = false,
    ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        },
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(
            new HealthResult
            {
                Name = app.Environment.ApplicationName,
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Services = report.Entries.Select(e => new HealthServiceStatus
                {
                    Key = e.Key,
                    Description = e.Value.Description ?? String.Empty,
                    Duration = e.Value.Duration,
                    Status = Enum.GetName(typeof(HealthStatus),
                                            e.Value.Status) ?? String.Empty,
                    Error = e.Value.Exception?.Message,
                    Data = e.Value.Data.Select(k => k).ToList()
                }).ToList()
            },
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});


ILogger logger = app.Logger;
IHostApplicationLifetime lifetime = app.Lifetime;
IWebHostEnvironment env = app.Environment;

lifetime.ApplicationStarted.Register(() =>
    logger.LogInformation($"The application {env.ApplicationName} started"));

// ffmpeg
Program.FFmpegLogger = app.Services.GetRequiredService<FFmpegLogger>();

app.Run();


partial class Program
{
    public static string InstanceName = "Default";
    public static readonly string ServiceName = "MediaInfo.Service";
    private static FFmpegLogger? FFmpegLogger { get; set; }

    public static void PrintProductVersion()
    {
        var assembly = typeof(Program).Assembly;
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Starting {product} v{version}...");
        Console.ResetColor();
    } 
}