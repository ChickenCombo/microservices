using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.DTOs;
using PlatformService.Interfaces;
using PlatformService.Models;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController(
    IPlatformRepository platformRepository,
    IMapper mapper
) : ControllerBase
{
    private readonly IPlatformRepository _platformRepository = platformRepository;
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
    public ActionResult<PlatformReadDto> CreatePlatform(PlatformCreateDto platformCreateDto)
    {
        var platformModel = _mapper.Map<Platform>(platformCreateDto);

        _platformRepository.CreatePlatform(platformModel);
        _platformRepository.SaveChanges();

        var platformDto = _mapper.Map<PlatformReadDto>(platformModel);

        return CreatedAtRoute(nameof(GetPlatformById), new { platformDto.Id }, platformDto);
    }
}