using PlatformService.Data;
using PlatformService.Interfaces;
using PlatformService.Models;

namespace PlatformService.Repositories;

public class PlatformRepository(AppDbContext context) : IPlatformRepository
{
    private readonly AppDbContext _context = context;

    public void CreatePlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        _context.Platforms.Add(platform);
    }

    public IEnumerable<Platform> GetAllPlatforms()
    {
        return _context.Platforms.ToList();
    }

    public Platform? GetPlatformById(int id)
    {
        return _context.Platforms.FirstOrDefault(p => p.Id == id);
    }

    public bool SaveChanges()
    {
        return _context.SaveChanges() >= 0;
    }
}