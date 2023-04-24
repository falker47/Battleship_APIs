using Battleship_APIs.Models;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var localConfig = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json", true, false).Build();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "Policy",
                policy =>
                {
                    policy.WithOrigins("http://localhost:4200",
                                        "https://polite-sky-0deac7e03.3.azurestaticapps.net")
                            .WithMethods("POST", "GET")
                            .AllowAnyHeader();
                });
        });

        builder.Services.AddDbContext<BattleshipDbContext>(options => options.UseSqlServer(localConfig.GetConnectionString("localConnectionString")));
        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}