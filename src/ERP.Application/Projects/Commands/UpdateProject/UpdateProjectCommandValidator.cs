using FluentValidation;

namespace ERP.Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(v => v.Id)
                .GreaterThan(0).WithMessage("Project ID must be greater than 0.");

            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

            RuleFor(v => v.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

            RuleFor(v => v.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(v => v.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

            RuleFor(v => v.Budget)
                .GreaterThan(0).WithMessage("Budget must be greater than 0.");

            RuleFor(v => v.Progress)
                .InclusiveBetween(0, 100).WithMessage("Progress must be between 0 and 100.");

            RuleFor(v => v.CustomerId)
                .GreaterThan(0).WithMessage("Customer ID must be greater than 0.");

            RuleFor(v => v.ProjectManagerId)
                .GreaterThan(0).WithMessage("Project Manager ID must be greater than 0.");

            When(v => v.ActualStartDate.HasValue, () =>
            {
                RuleFor(v => v.ActualStartDate)
                    .LessThanOrEqualTo(DateTime.Now).WithMessage("Actual start date cannot be in the future.");
            });

            When(v => v.ActualEndDate.HasValue, () =>
            {
                RuleFor(v => v.ActualEndDate)
                    .GreaterThan(x => x.ActualStartDate ?? x.StartDate)
                    .WithMessage("Actual end date must be after actual start date.");
            });
        }
    }
}
