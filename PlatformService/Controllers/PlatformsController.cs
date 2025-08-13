using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.DTOs;
using PlatformService.Interfaces;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController(
    IPlatformRepository platformRepository,
    ICommandDataClient commandDataClient,
    IMessageBusClient messageBusClient,
    IMapper mapper
) : ControllerBase
{
    private readonly IPlatformRepository _platformRepository = platformRepository;
    private readonly ICommandDataClient _commandDataClient = commandDataClient;
    private readonly IMessageBusClient _messageBusClient = messageBusClient;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetAllPlatforms()
    {
        var platforms = _platformRepository.GetAllPlatforms();
        var platformsDto = _mapper.Map<IEnumerable<PlatformReadDto>>(platforms);

        return Ok(platformsDto);
    }

    [HttpGet("{id:int}", Name = "GetPlatformById")]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        var platform = _platformRepository.GetPlatformById(id);

        if (platform == null)
        {
            return NotFound();
        }

        var platformDto = _mapper.Map<Platform>(platform);

        return Ok(platformDto);
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
    {
        var platformModel = _mapper.Map<Platform>(platformCreateDto);

        _platformRepository.CreatePlatform(platformModel);
        _platformRepository.SaveChanges();

        var platformDto = _mapper.Map<PlatformReadDto>(platformModel);

        // Send synchronous message
        try
        {
            await _commandDataClient.SendPlatformToCommand(platformDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
        }

        // Send asynchronous message
        try
        {
            var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformDto);
            platformPublishedDto.Event = "Platform_Published";

            await _messageBusClient.PublishNewPlatform(platformPublishedDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
        }

        return CreatedAtRoute(nameof(GetPlatformById), new { platformDto.Id }, platformDto);
    }
}