using AutoMapper;
using AutoMapper.QueryableExtensions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Projects.Queries.GetProjects
{
    public class GetProjectsQuery : IRequest<PaginatedList<ProjectDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProjectStatus? Status { get; set; }
        public int? CustomerId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
