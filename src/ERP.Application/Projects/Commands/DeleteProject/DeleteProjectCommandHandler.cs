using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;

namespace ERP.Application.Projects.Commands.DeleteProject
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProjectCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Projects
                .Where(p => p.CompanyId == _currentUserService.CompanyId)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Project), request.Id);
            }

            // Check if project has associated time entries or invoices
            var hasTimeEntries = await _context.TimeEntries
                .AnyAsync(t => t.ProjectId == request.Id, cancellationToken);

            var hasInvoices = await _context.Invoices
                .AnyAsync(i => i.ProjectId == request.Id, cancellationToken);

            if (hasTimeEntries || hasInvoices)
            {
                // Soft delete if project has associated data
                entity.IsDeleted = true;

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
            }
            else
            {
                // Hard delete if no associated data
                _context.Projects.Remove(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
