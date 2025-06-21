namespace ERP.Application.Common.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// 단일 수신자에게 이메일을 발송합니다.
        /// </summary>
        /// <param name="to">수신자 이메일</param>
        /// <param name="subject">제목</param>
        /// <param name="body">내용</param>
        /// <param name="isHtml">HTML 형식 여부</param>
        /// <returns>발송 성공 여부</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// 여러 수신자에게 이메일을 발송합니다.
        /// </summary>
        /// <param name="to">주 수신자</param>
        /// <param name="subject">제목</param>
        /// <param name="body">내용</param>
        /// <param name="cc">참조 수신자</param>
        /// <param name="bcc">숨은참조 수신자</param>
        /// <param name="isHtml">HTML 형식 여부</param>
        /// <returns>발송 성공 여부</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null, bool isHtml = true);
    }
}
