using AutoMapper;
using CommandsService.DTOs;
using CommandsService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers;

[Route("api/c/[controller]")]
[ApiController]
public class PlatformsController(ICommandRepository repository, IMapper mapper) : ControllerBase
{
    private readonly ICommandRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        var platforms = _repository.GetAllPlatforms();
        var platformsDto = _mapper.Map<IEnumerable<PlatformReadDto>>(platforms);

        return Ok(platformsDto);
    }

    [HttpPost]
    public ActionResult TestInboundConnection()
    {
        Console.WriteLine("--> Inbound POST # Command Service");

        return Ok("Inbound Test");
    }
}