namespace ERP.Application.Common.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// 특정 사용자에게 실시간 알림을 전송합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="message">알림 메시지</param>
        /// <param name="type">알림 타입 (info, warning, error, success)</param>
        Task SendNotificationAsync(string userId, string message, string type = "info");

        /// <summary>
        /// 특정 그룹에게 실시간 알림을 전송합니다.
        /// </summary>
        /// <param name="groupName">그룹명</param>
        /// <param name="message">알림 메시지</param>
        /// <param name="type">알림 타입</param>
        Task SendNotificationToGroupAsync(string groupName, string message, string type = "info");

        /// <summary>
        /// 모든 사용자에게 실시간 알림을 전송합니다.
        /// </summary>
        /// <param name="message">알림 메시지</param>
        /// <param name="type">알림 타입</param>
        Task SendNotificationToAllAsync(string message, string type = "info");
    }
}
