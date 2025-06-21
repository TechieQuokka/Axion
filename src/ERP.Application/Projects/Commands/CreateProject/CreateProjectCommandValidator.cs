using FluentValidation;
using ERP.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Projects.Commands.CreateProject
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateProjectCommandValidator(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (currentUserService == null) throw new ArgumentNullException(nameof(currentUserService));

            _context = context;
            _currentUserService = currentUserService;

            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("프로젝트명은 필수입니다.")
                .MaximumLength(200).WithMessage("프로젝트명은 200자를 초과할 수 없습니다.");

            RuleFor(v => v.Code)
                .NotEmpty().WithMessage("프로젝트 코드는 필수입니다.")
                .MaximumLength(50).WithMessage("프로젝트 코드는 50자를 초과할 수 없습니다.")
                .MustAsync(BeUniqueCode).WithMessage("이미 사용중인 프로젝트 코드입니다.");

            RuleFor(v => v.StartDate)
                .NotEmpty().WithMessage("시작일은 필수입니다.");

            RuleFor(v => v.EndDate)
                .NotEmpty().WithMessage("종료일은 필수입니다.")
                .GreaterThan(v => v.StartDate).WithMessage("종료일은 시작일보다 늦어야 합니다.");

            RuleFor(v => v.Budget)
                .GreaterThan(0).WithMessage("예산은 0보다 커야 합니다.");

            RuleFor(v => v.CustomerId)
                .MustAsync(CustomerExists).WithMessage("존재하지 않는 고객입니다.");

            RuleFor(v => v.ProjectManagerId)
                .MustAsync(UserExists).WithMessage("존재하지 않는 사용자입니다.");
        }

        public async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false; // Empty or null code is not unique
            }
            return await _context.Projects
                .Where(p => p.CompanyId == _currentUserService.CompanyId)
                .AllAsync(p => p.Code != code, cancellationToken);
        }

        public async Task<bool> CustomerExists(int customerId, CancellationToken cancellationToken)
        {
            return await _context.Customers
                .Where(c => c.CompanyId == _currentUserService.CompanyId)
                .AnyAsync(c => c.Id == customerId, cancellationToken);
        }

        public async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.CompanyId == _currentUserService.CompanyId)
                .AnyAsync(u => u.Id == userId, cancellationToken);
        }
    }
}
