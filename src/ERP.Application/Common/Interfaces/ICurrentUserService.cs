namespace ERP.Application.Common.Interfaces
{
    /// <summary>
    /// Provides information about the currently authenticated user, such as user ID, company ID, and authentication status.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the Identity system user ID (string) - GUID 형태의 로그인 사용자 ID
        /// </summary>
        string IdentityUserId { get; }

        /// <summary>
        /// Gets the business domain user ID (int) - ERP 시스템 내부의 사용자 ID
        /// </summary>
        int BusinessUserId { get; }

        /// <summary>
        /// Gets the business domain user ID (int) - BusinessUserId와 동일 (하위 호환성)
        /// </summary>
        int UserId => BusinessUserId;

        /// <summary>
        /// Gets the company ID associated with the current user.
        /// </summary>
        int CompanyId { get; }

        /// <summary>
        /// Gets the username or display name of the current user.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Indicates whether the current user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Checks if the current user has the specified role.
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <returns>True if the user has the role, false otherwise</returns>
        bool IsInRole(string role);

        /// <summary>
        /// Checks if the current user has the specified permission.
        /// </summary>
        /// <param name="permission">The permission to check</param>
        /// <returns>True if the user has the permission, false otherwise</returns>
        bool HasPermission(string permission);
    }
}