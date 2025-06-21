using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using MediatR;

namespace ERP.Application.Projects.Commands.CreateProject
{
    public class CreateProjectCommand : IRequest<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ProjectType Type { get; set; }
        public Priority Priority { get; set; }
        public decimal Budget { get; set; }
        public int CustomerId { get; set; }
        public int ProjectManagerId { get; set; }
    }

    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateProjectCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<int> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var entity = new Project
            {
                CompanyId = _currentUserService.CompanyId,
                Name = request.Name,
                Description = request.Description,
                Code = request.Code,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Type = request.Type,
                Priority = request.Priority,
                Budget = request.Budget,
                CustomerId = request.CustomerId,
                ProjectManagerId = request.ProjectManagerId,
                Status = ProjectStatus.Planning,
                ActualCost = 0,
                Progress = 0,
                // CreatedBy와 CreatedByUserId는 ApplicationDbContext.SaveChangesAsync에서 자동 설정됨
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
