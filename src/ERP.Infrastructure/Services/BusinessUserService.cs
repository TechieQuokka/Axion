using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services
{
    public class BusinessUserService : IBusinessUserService
    {
        private readonly IApplicationDbContext _context;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public BusinessUserService(
            IApplicationDbContext context,
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == _currentUserService.CompanyId, CancellationToken.None);
        }

        public async Task<int> CreateUserAsync(User user, string identityUserId)
        {
            user.CompanyId = _currentUserService.CompanyId;
            user.CreatedBy = identityUserId;
            user.CreatedAt = _dateTime.UtcNow;
            user.UpdatedBy = identityUserId;
            user.UpdatedAt = _dateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync(CancellationToken.None);

            // ApplicationUser에 BusinessUserId 설정 - ApplicationDbContext 사용
            var applicationUser = await _dbContext.Set<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.Id == identityUserId, CancellationToken.None);

            if (applicationUser != null)
            {
                applicationUser.BusinessUserId = user.Id;
                await _dbContext.SaveChangesAsync(CancellationToken.None);
            }

            return user.Id;
        }

        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedBy = _currentUserService.IdentityUserId;
            user.UpdatedAt = _dateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeactivateUserAsync(int userId)
        {
            var user = await GetUserAsync(userId);
            if (user != null)
            {
                user.Status = Domain.Enums.UserStatus.Inactive;
                user.UpdatedBy = _currentUserService.IdentityUserId;
                user.UpdatedAt = _dateTime.UtcNow;

                await _context.SaveChangesAsync(CancellationToken.None);
            }
        }

        public async Task<List<User>> GetUsersByCompanyAsync(int companyId)
        {
            return await _context.Users
                .Where(u => u.CompanyId == companyId && !u.IsDeleted)
                .ToListAsync(CancellationToken.None);
        }

        public async Task<User?> GetUserByIdentityIdAsync(string identityUserId)
        {
            // ApplicationUser에서 BusinessUserId 조회 - ApplicationDbContext 사용
            var applicationUser = await _dbContext.Set<ApplicationUser>()
                .Where(u => u.Id == identityUserId)
                .Select(u => new { u.BusinessUserId, u.Email, u.CompanyId })
                .FirstOrDefaultAsync(CancellationToken.None);

            if (applicationUser?.BusinessUserId.HasValue == true)
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == applicationUser.BusinessUserId.Value, CancellationToken.None);
            }

            // BusinessUserId가 없으면 Email로 찾기
            if (applicationUser != null && !string.IsNullOrEmpty(applicationUser.Email))
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == applicationUser.Email &&
                                            u.CompanyId == applicationUser.CompanyId, CancellationToken.None);
            }

            return null;
        }
    }
}
