using _MediaInfoService.Models;
using FFmpeg.AutoGen;
using Microsoft.OpenApi.Models;

namespace _MediaInfoService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure CorsPolicy (AllowAnyOrigin, AllowAnyMethod, AllowAnyHeader)
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddServices(this IServiceCollection services)
        {
            // BackgroundTaskQueue
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            // FFmpeg
            services.AddSingleton<FFmpegLogger>(
                ffmpegLogger => new FFmpegLogger(logger: ffmpegLogger.GetRequiredService<ILogger<FFmpegLogger>>(), level: ffmpeg.AV_LOG_VERBOSE, flags: ffmpeg.AV_LOG_SKIP_REPEATED)
            );
        }

        /// <summary>
        /// Register the Swagger generator, defining 1 or more Swagger documents
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "MediaInfo Service",
                    Description = "REST control interface for MediaInfo Service",
                    TermsOfService = new Uri("https://freehand.com.ua/projects/_mediaInfo-service/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Oleksandr Nazaruk",
                        Email = "mail@freehand.com.ua",
                        Url = new Uri("https://github.com/freehand-dev"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://freehand.com.ua/projects/_mediaInfo-service/license"),
                    }
                });
            });
        }
    }
}
