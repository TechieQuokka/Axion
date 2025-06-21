using MediatR;
using ERP.Domain.Enums;

namespace ERP.Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
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
        public int CustomerId { get; set; }
        public int ProjectManagerId { get; set; }
    }
}
