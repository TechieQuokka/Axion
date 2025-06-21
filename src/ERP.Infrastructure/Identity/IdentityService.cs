using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ERP.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsPrincipalFactory;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuthorizationService authorizationService,
            ICurrentUserService currentUserService,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsPrincipalFactory)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
            _currentUserService = currentUserService;
            _claimsPrincipalFactory = claimsPrincipalFactory;
        }

        public async Task<string?> GetUserNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName;
        }

        public async Task<string?> GetUserIdAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user?.Id;
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> AuthorizeAsync(string userId, string policyName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var principal = await CreateClaimsPrincipalAsync(user);
            var result = await _authorizationService.AuthorizeAsync(principal, policyName);

            return result.Succeeded;
        }

        public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = userName,
                CompanyId = _currentUserService.CompanyId
            };

            var result = await _userManager.CreateAsync(user, password);

            return (result.ToApplicationResult(), user.Id);
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            return user != null
                ? (await _userManager.DeleteAsync(user)).ToApplicationResult()
                : Result.Success();
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null
                ? await _userManager.GetRolesAsync(user)
                : new List<string>();
        }

        public async Task<Result> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(new[] { "User not found." });

            // 역할이 존재하지 않으면 생성
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.ToApplicationResult();
        }

        public async Task<Result> RemoveFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(new[] { "User not found." });

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.ToApplicationResult();
        }

        public async Task<bool> CheckPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(new[] { "User not found." });

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.ToApplicationResult();
        }

        public async Task<Result> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(new[] { "User not found." });

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.ToApplicationResult();
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(userId));

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(userId));

            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<Result> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(new[] { "User not found." });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.ToApplicationResult();
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsEmailConfirmedAsync(user);
        }

        private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user)
        {
            return await _claimsPrincipalFactory.CreateAsync(user);
        }
    }

    // IdentityResult 확장 메서드
    public static class IdentityResultExtensions
    {
        public static Result ToApplicationResult(this IdentityResult result)
        {
            return result.Succeeded
                ? Result.Success()
                : Result.Failure(result.Errors.Select(e => e.Description));
        }
    }
}
