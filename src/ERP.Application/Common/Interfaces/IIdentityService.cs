using ERP.Application.Common.Models;

namespace ERP.Application.Common.Interfaces
{
    /// <summary>
    /// Provides an abstraction layer for user identity management, including authentication, authorization, role management, and password operations.
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Gets the user name for the specified Identity user ID (string).
        /// </summary>
        Task<string?> GetUserNameAsync(string userId);

        /// <summary>
        /// Checks if the user belongs to the specified role.
        /// </summary>
        Task<bool> IsInRoleAsync(string userId, string role);

        /// <summary>
        /// Authorizes the user for a given policy.
        /// </summary>
        Task<bool> AuthorizeAsync(string userId, string policyName);

        /// <summary>
        /// Creates a new user with the given user name and password.
        /// Returns Identity UserId (string).
        /// </summary>
        Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

        /// <summary>
        /// Deletes the user identified by the Identity user ID (string).
        /// </summary>
        Task<Result> DeleteUserAsync(string userId);

        /// <summary>
        /// Retrieves the Identity user ID (string) associated with the given user name.
        /// </summary>
        Task<string?> GetUserIdAsync(string userName);

        /// <summary>
        /// Gets the list of roles assigned to the specified Identity user ID (string).
        /// </summary>
        Task<IList<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Adds the specified user to the given role.
        /// </summary>
        Task<Result> AddToRoleAsync(string userId, string role);

        /// <summary>
        /// Removes the specified user from the given role.
        /// </summary>
        Task<Result> RemoveFromRoleAsync(string userId, string role);

        /// <summary>
        /// Verifies the user's password.
        /// </summary>
        Task<bool> CheckPasswordAsync(string userId, string password);

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Resets the user's password using a reset token.
        /// </summary>
        Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);

        /// <summary>
        /// Generates a password reset token for the specified user.
        /// </summary>
        Task<string> GeneratePasswordResetTokenAsync(string userId);

        /// <summary>
        /// Generates an email confirmation token for the specified user.
        /// </summary>
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);

        /// <summary>
        /// Confirms the user's email using the given token.
        /// </summary>
        Task<Result> ConfirmEmailAsync(string userId, string token);

        /// <summary>
        /// Checks if the user's email is confirmed.
        /// </summary>
        Task<bool> IsEmailConfirmedAsync(string userId);
    }
}
