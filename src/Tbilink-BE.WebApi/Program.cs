using Tbilink_BE.Application;
using Tbilink_BE.Domain;
using Tbilink_BE.Infrastructure;
using Tbilink_BE.WebApi;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddApplicationServices();
        builder.AddDomainServices();
        builder.AddInfrastuctureServices();
        builder.AddWebApiServices();

        var app = builder.Build();

        app.ConfigureMiddleware();

        app.Run();
    }
}