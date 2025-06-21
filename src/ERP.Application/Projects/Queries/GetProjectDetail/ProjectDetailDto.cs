using AutoMapper;
using ERP.Application.Common.Mappings;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.Projects.Queries.GetProjectDetail
{
    public class ProjectDetailDto : IMapFrom<Project>
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Customer Information
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerContactName { get; set; } = string.Empty;
        public string CustomerContactEmail { get; set; } = string.Empty;

        // Project Manager Information
        public int ProjectManagerId { get; set; }
        public string ProjectManagerName { get; set; } = string.Empty;
        public string ProjectManagerEmail { get; set; } = string.Empty;

        // Project Statistics
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ActiveMembers { get; set; }
        public decimal TotalHoursLogged { get; set; }
        public decimal BudgetUtilization => Budget > 0 ? (ActualCost / Budget) * 100 : 0;

        // Project Members
        public List<ProjectMemberDto> Members { get; set; } = new();

        // Recent Tasks
        public List<ProjectTaskDto> RecentTasks { get; set; } = new();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Project, ProjectDetailDto>()
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
                .ForMember(d => d.CustomerContactName, opt => opt.MapFrom(s => s.Customer.ContactName))
                .ForMember(d => d.CustomerContactEmail, opt => opt.MapFrom(s => s.Customer.ContactEmail))
                .ForMember(d => d.ProjectManagerName, opt => opt.MapFrom(s => $"{s.ProjectManager.FirstName} {s.ProjectManager.LastName}"))
                .ForMember(d => d.ProjectManagerEmail, opt => opt.MapFrom(s => s.ProjectManager.Email))
                .ForMember(d => d.TotalTasks, opt => opt.MapFrom(s => s.Tasks.Count))
                .ForMember(d => d.CompletedTasks, opt => opt.MapFrom(s => s.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)))
                .ForMember(d => d.ActiveMembers, opt => opt.MapFrom(s => s.Members.Count(m => m.UnassignedDate == null)))
                .ForMember(d => d.TotalHoursLogged, opt => opt.MapFrom(s => s.TimeEntries.Sum(t => t.Hours)))
                .ForMember(d => d.Members, opt => opt.MapFrom(s => s.Members.Where(m => m.UnassignedDate == null)))
                .ForMember(d => d.RecentTasks, opt => opt.MapFrom(s => s.Tasks.OrderByDescending(t => t.UpdatedAt).Take(5)));
        }
    }

    public class ProjectMemberDto : IMapFrom<ProjectMember>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
        public int AllocationPercentage { get; set; }
        public DateTime AssignedDate { get; set; }
        public decimal? HourlyRate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
                .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User.Email));
        }
    }

    public class ProjectTaskDto : IMapFrom<ProjectTask>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Domain.Enums.TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProjectTask, ProjectTaskDto>()
                .ForMember(d => d.AssigneeName, opt => opt.MapFrom(s =>
                    s.Assignee != null ? $"{s.Assignee.FirstName} {s.Assignee.LastName}" : string.Empty));
        }
    }
}
