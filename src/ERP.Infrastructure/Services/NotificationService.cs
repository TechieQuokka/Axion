using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string userId, string message, string type = "info")
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    message,
                    type,
                    timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Notification sent to user {UserId}: {Message}", userId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToGroupAsync(string groupName, string message, string type = "info")
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
                {
                    message,
                    type,
                    timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Notification sent to group {GroupName}: {Message}", groupName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to group {GroupName}", groupName);
            }
        }

        public async Task SendNotificationToAllAsync(string message, string type = "info")
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    message,
                    type,
                    timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Notification sent to all users: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to all users");
            }
        }
    }
}
