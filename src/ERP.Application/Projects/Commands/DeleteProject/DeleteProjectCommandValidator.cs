using FluentValidation;

namespace ERP.Application.Projects.Commands.DeleteProject
{
    public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
    {
        public DeleteProjectCommandValidator()
        {
            RuleFor(v => v.Id)
                .GreaterThan(0).WithMessage("Project ID must be greater than 0.");
        }
    }
}
