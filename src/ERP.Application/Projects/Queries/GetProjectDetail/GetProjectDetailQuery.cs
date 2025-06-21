using MediatR;

namespace ERP.Application.Projects.Queries.GetProjectDetail
{
    public class GetProjectDetailQuery : IRequest<ProjectDetailDto>
    {
        public int Id { get; set; }
    }
}
