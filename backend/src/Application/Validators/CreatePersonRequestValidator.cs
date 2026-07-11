using FluentValidation;
using Application.DTOs;

namespace Application.Validators;

public class CreatePersonRequestValidator : AbstractValidator<CreatePersonRequest>
{
    public CreatePersonRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 150).WithMessage("Idade deve ser entre 1 e 150 anos.");
    }
}
