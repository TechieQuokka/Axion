using FluentValidation;

namespace ERP.Application.Projects.Queries.GetProjectDetail
{
    public class GetProjectDetailQueryValidator : AbstractValidator<GetProjectDetailQuery>
    {
        public GetProjectDetailQueryValidator()
        {
            RuleFor(v => v.Id)
                .GreaterThan(0).WithMessage("Project ID must be greater than 0.");
        }
    }
}
