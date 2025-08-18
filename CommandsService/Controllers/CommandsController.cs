using AutoMapper;
using CommandsService.DTOs;
using CommandsService.Interfaces;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers;

[Route("api/c/platforms/{platformId:int}/[controller]")]
[ApiController]
public class CommandsController(ICommandRepository repository, IMapper mapper) : ControllerBase
{
    private readonly ICommandRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
    {
        if (!_repository.PlatformExist(platformId))
        {
            return NotFound();
        }

        var commands = _repository.GetCommandsForPlatform(platformId);
        var commandsDto = _mapper.Map<IEnumerable<CommandReadDto>>(commands);

        return Ok(commandsDto);
    }

    [HttpGet("{commandId:int}", Name = "GetCommandForPlatform")]
    public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
    {
        if (!_repository.PlatformExist(platformId))
        {
            return NotFound();
        }

        var command = _repository.GetCommand(platformId, commandId);

        if (command == null)
        {
            return NotFound();
        }

        var commandDto = _mapper.Map<CommandReadDto>(command);

        return Ok(commandDto);
    }

    [HttpPost]
    public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, [FromBody] CommandCreateDto commandCreateDto)
    {
        if (!_repository.PlatformExist(platformId))
        {
            return NotFound();
        }

        var command = _mapper.Map<Command>(commandCreateDto);

        _repository.CreateCommand(platformId, command);
        _repository.SaveChanges();

        var commandDto = _mapper.Map<CommandReadDto>(command);

        return CreatedAtRoute(nameof(GetCommandForPlatform), new { platformId, commandId = command.Id }, commandDto);
    }
}