using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Tbilink_BE.WebApi.signalR;

namespace Tbilink_BE.WebApi
{
    public static class MiddlewareConfiguration
    {
        public static void ConfigureMiddleware(this WebApplication app)
        {
        //    // Localization
        //    var supportedCultures = new[]
        //    {
        //    new CultureInfo("en-US"),
        //    new CultureInfo("ru-RU"),
        //    new CultureInfo("ka-GE")
        //};

        //    var localizationOptions = new RequestLocalizationOptions
        //    {
        //        DefaultRequestCulture = new RequestCulture("en-US"),
        //        SupportedCultures = supportedCultures,
        //        SupportedUICultures = supportedCultures
        //    };

        //    localizationOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());

            // Dev

            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tbilink API V1");
            });

            app.UseCors("AppCorsPolicy");

            // Middlewares
            //app.UseRequestLocalization(localizationOptions);
            //app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            //app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseAuthorization();

            // For wwwroot
            //app.UseStaticFiles();


            app.MapControllers();

            app.MapHub<UserHub>("/hubs/users");
        }
    }
}
