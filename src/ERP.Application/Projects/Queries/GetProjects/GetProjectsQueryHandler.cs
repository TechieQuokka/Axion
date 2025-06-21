using AutoMapper;
using AutoMapper.QueryableExtensions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Projects.Queries.GetProjects
{
    public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PaginatedList<ProjectDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetProjectsQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PaginatedList<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectManager)
                .AsQueryable();

            // 수동으로 CompanyId 필터링 (Global Query Filter가 적용되지 않는 경우를 대비)
            if (_currentUserService.IsAuthenticated && _currentUserService.CompanyId > 0)
            {
                query = query.Where(p => p.CompanyId == _currentUserService.CompanyId);
            }

            // IsDeleted 필터링
            query = query.Where(p => !p.IsDeleted);

            // 추가 필터 적용
            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status.Value);
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == request.CustomerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(request.SearchTerm) ||
                    p.Code.Contains(request.SearchTerm) ||
                    p.Description.Contains(request.SearchTerm));
            }

            return await PaginatedList<ProjectDto>.CreateAsync(
                query.OrderByDescending(p => p.CreatedAt)
                     .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
