using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;

namespace ERP.Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProjectCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Projects
                .Where(p => p.CompanyId == _currentUserService.CompanyId)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Project), request.Id);
            }

            // Check if customer exists and belongs to the same company
            var customerExists = await _context.Customers
                .AnyAsync(c => c.Id == request.CustomerId &&
                              c.CompanyId == _currentUserService.CompanyId,
                         cancellationToken);

            if (!customerExists)
            {
                throw new NotFoundException(nameof(Customer), request.CustomerId);
            }

            // Check if project manager exists and belongs to the same company
            var managerExists = await _context.Users
                .AnyAsync(u => u.Id == request.ProjectManagerId &&
                              u.CompanyId == _currentUserService.CompanyId,
                         cancellationToken);

            if (!managerExists)
            {
                throw new NotFoundException("Project Manager", request.ProjectManagerId);
            }

            // Update entity properties
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.ActualStartDate = request.ActualStartDate;
            entity.ActualEndDate = request.ActualEndDate;
            entity.Status = request.Status;
            entity.Type = request.Type;
            entity.Priority = request.Priority;
            entity.Budget = request.Budget;
            entity.ActualCost = request.ActualCost;
            entity.Progress = request.Progress;
            entity.CustomerId = request.CustomerId;
            entity.ProjectManagerId = request.ProjectManagerId;

            // UpdatedBy는 string 타입이므로 IdentityUserId 사용
            entity.UpdatedBy = _currentUserService.IdentityUserId;

            // UpdatedByUserId는 int? 타입이므로 BusinessUserId 사용 (선택적)
            if (_currentUserService.IsAuthenticated)
            {
                try
                {
                    entity.UpdatedByUserId = _currentUserService.BusinessUserId;
                }
                catch
                {
                    // BusinessUserId 조회 실패 시 null로 유지
                    entity.UpdatedByUserId = null;
                }
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
