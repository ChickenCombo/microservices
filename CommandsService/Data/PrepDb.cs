using Microsoft.EntityFrameworkCore;

namespace CommandsService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        SeedData(context);
    }

    private static void SeedData(AppDbContext context)
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


        if (!context.Platforms.Any())
        {
            Console.WriteLine("--> Seeding database");

            context.SaveChanges();
        }
        else
        {
            Console.WriteLine("--> Skipping seeder");
        }
    }
}