using CommandsService.Interfaces;
using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;

namespace CommandsService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

        var platforms = grpcClient!.ReturnAllPlatforms();

        SeedData(serviceScope.ServiceProvider.GetService<ICommandRepository>()!, platforms, context);
    }

    private static void SeedData(ICommandRepository repo, IEnumerable<Platform> platforms, AppDbContext context)
    {
        try
        {
            Console.WriteLine("--> Attempting to apply database migrations");
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Failed to apply migrations: {ex.Message}");
        }


        Console.WriteLine("--> Seeding database");

        foreach (var plat in platforms)
        {
            if (!repo.ExternalPlatformExist(plat.ExternalId))
            {
                repo.CreatePlatform(plat);
            }
        }

        repo.SaveChanges();
    }
}