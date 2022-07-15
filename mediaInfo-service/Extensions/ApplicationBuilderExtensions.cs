using _MediaInfoService.Middlewares;
using Microsoft.Extensions.FileProviders;


namespace _MediaInfoService.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureStaticFiles(this IApplicationBuilder app, string workDirectory)
        {

            // configure UI StaticFiles
            var _uiPath = System.IO.Path.Combine(workDirectory, "ui");
            // create UI Directory if not exists
            System.IO.Directory.CreateDirectory(_uiPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(_uiPath),
                RequestPath = "/ui"
            });
        }
        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
        }

    }
}
