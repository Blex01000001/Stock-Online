using Stock_Online.Application.DTOs.Commands;

namespace Stock_Online.Application.Services.UpdateOrchestrator
{
    public interface IUpdateOrchestrator
    {
        Task<string> QueueUpdateJobAsync(UpdateCommand command);
    }
}
