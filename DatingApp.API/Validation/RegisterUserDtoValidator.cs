using DatingApp.API.Dtos;
using DatingApp.API.Shared;
using FluentValidation;

namespace DatingApp.API.Validation
{
    /// <summary>
    /// Messages are serialized <see cref="Error"/> objects, not prose.
    /// <see cref="ModelStateValidator"/> unpacks them into { code, message, invalidField }.
    /// </summary>
    public sealed class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("username").Serialize());

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("password").Serialize())
                .MinimumLength(4)
                    .WithMessage(Error.Errors.General.MinLengthError(4, "password").Serialize())
                .MaximumLength(8)
                    .WithMessage(Error.Errors.General.MaxLengthError(8, "password").Serialize());
        }
    }
}
