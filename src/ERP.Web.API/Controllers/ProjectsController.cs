using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Common.Models;
using ERP.Application.Projects.Commands.CreateProject;
using ERP.Application.Projects.Commands.UpdateProject;
using ERP.Application.Projects.Commands.DeleteProject;
using ERP.Application.Projects.Queries.GetProjects;
using ERP.Application.Projects.Queries.GetProjectDetail;

namespace ERP.Web.API.Controllers
{
    // 일시적으로 인증 제거 (테스트용)
    [AllowAnonymous]
    public class ProjectsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ProjectDto>>> GetProjects([FromQuery] GetProjectsQuery query)
        {
            return await Mediator.Send(query);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDetailDto>> GetProject(int id)
        {
            return await Mediator.Send(new GetProjectDetailQuery { Id = id });
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateProjectCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateProjectCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await Mediator.Send(command);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await Mediator.Send(new DeleteProjectCommand { Id = id });

            return NoContent();
        }
    }
}
