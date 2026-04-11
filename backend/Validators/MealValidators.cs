using FluentValidation;
using Tambouille.DTOs;

namespace Tambouille.Validators;

public class CreateMealRequestValidator : AbstractValidator<CreateMealRequest>
{
    public CreateMealRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Date must be in ISO 8601 format (yyyy-MM-dd)");

        RuleFor(x => x.Slot)
            .InclusiveBetween(0, 1).WithMessage("Slot must be 0 (midi) or 1 (soir)");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Persons)
            .GreaterThan(0).WithMessage("Persons must be at least 1");
    }
}

public class UpdateMealRequestValidator : AbstractValidator<UpdateMealRequest>
{
    public UpdateMealRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Persons)
            .GreaterThan(0).WithMessage("Persons must be at least 1");
    }
}
