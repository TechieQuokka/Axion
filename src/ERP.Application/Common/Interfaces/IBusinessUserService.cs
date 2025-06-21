using ERP.Domain.Entities;

namespace ERP.Application.Common.Interfaces
{
    /// <summary>
    /// Business User 관리를 위한 서비스 (ERP Domain Users)
    /// </summary>
    public interface IBusinessUserService
    {
        /// <summary>
        /// Gets the business user by ID.
        /// </summary>
        Task<User?> GetUserAsync(int userId);

        /// <summary>
        /// Creates a new business user linked to an Identity user.
        /// </summary>
        Task<int> CreateUserAsync(User user, string identityUserId);

        /// <summary>
        /// Updates business user information.
        /// </summary>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Deactivates a business user (soft delete).
        /// </summary>
        Task DeactivateUserAsync(int userId);

        /// <summary>
        /// Gets business users by company.
        /// </summary>
        Task<List<User>> GetUsersByCompanyAsync(int companyId);

        /// <summary>
        /// Finds business user by Identity user ID.
        /// </summary>
        Task<User?> GetUserByIdentityIdAsync(string identityUserId);
    }
}
