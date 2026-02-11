using Stock_Online.DTOs.UpdateRequest;

namespace Stock_Online.Services.UpdateOrchestrator
{
    public interface IUpdateOrchestrator
    {
        Task<string> QueueUpdateJobAsync(UpdateCommand command);
    }
}
