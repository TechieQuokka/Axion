using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ERP.Web.API.Hubs
{
    [Authorize]
    public class ProjectHub : Hub
    {
        private readonly ILogger<ProjectHub> _logger;

        public ProjectHub(ILogger<ProjectHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinProject(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
            _logger.LogInformation($"User {Context.UserIdentifier} joined project {projectId}");
        }

        public async Task LeaveProject(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
            _logger.LogInformation($"User {Context.UserIdentifier} left project {projectId}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"User {Context.UserIdentifier} connected");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"User {Context.UserIdentifier} disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
