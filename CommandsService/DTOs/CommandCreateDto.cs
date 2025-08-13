namespace CommandsService.DTOs;

public class CommandCreateDto
{
    public required string HowTo { get; set; }

    public required string CommandLine { get; set; }
}