using AutoMapper;
using ERP.Application.Common.Mappings;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.Projects.Queries.GetProjects
{
    public class ProjectDto : IMapFrom<Project>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public ProjectType Type { get; set; }
        public Priority Priority { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }
        public int Progress { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProjectManagerName { get; set; } = string.Empty;
        public bool IsOverBudget { get; set; }
        public bool IsDelayed { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Project, ProjectDto>()
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
                .ForMember(d => d.ProjectManagerName, opt => opt.MapFrom(s => s.ProjectManager.FullName));
        }
    }
}
