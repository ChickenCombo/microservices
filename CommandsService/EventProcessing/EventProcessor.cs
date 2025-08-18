using System.Text.Json;
using AutoMapper;
using CommandsService.DTOs;
using CommandsService.Interfaces;
using CommandsService.Models;

namespace CommandsService.EventProcessing;

enum EventType
{
    PlatformPublished,
    Undetermined,
}

public class EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper) : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IMapper _mapper = mapper;

    public void ProcessEevent(string message)
    {
        var eventType = DeterminedEvent(message);

        switch (eventType)
        {
            case EventType.PlatformPublished:
                break;
            default:
                break;
        }
    }

    private static EventType DeterminedEvent(string notificationMessage)
    {
        Console.WriteLine("--> Determining Event");

        var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

        return eventType?.Event switch
        {
            "Platform_Published" => EventType.PlatformPublished,
            _ => EventType.Undetermined,
        };
    }

    private void AddPlatform(string platformPublishedMessage)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

        var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

        try
        {
            var platform = _mapper.Map<Platform>(platformPublishedDto);

            if (!repo.ExternalPlatformExist(platform.ExternalId))
            {
                repo.CreatePlatform(platform);
                repo.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not add Platform to database: {ex.Message}");
        }
    }
}