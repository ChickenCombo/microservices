namespace CommandsService.EventProcessing;

public interface IEventProcessor
{
    void ProcessEevent(string message);
}