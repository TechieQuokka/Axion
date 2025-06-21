using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;

namespace ERP.Application.Projects.Queries.GetProjectDetail
{
    public class GetProjectDetailQueryHandler : IRequestHandler<GetProjectDetailQuery, ProjectDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetProjectDetailQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectDetailDto> Handle(GetProjectDetailQuery request, CancellationToken cancellationToken)
        {
            var project = await _context.Projects
                .Where(p => p.CompanyId == _currentUserService.CompanyId && p.Id == request.Id)
                .Include(p => p.Customer)
                .Include(p => p.ProjectManager)
                .Include(p => p.Members.Where(m => m.UnassignedDate == null))
                    .ThenInclude(m => m.User)
                .Include(p => p.Tasks.OrderByDescending(t => t.UpdatedAt).Take(5))
                    .ThenInclude(t => t.Assignee)
                .Include(p => p.TimeEntries)
                .ProjectTo<ProjectDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
            {
                throw new NotFoundException(nameof(Project), request.Id);
            }

            return project;
        }
    }
}
