using DatingApp.API.Dtos;
using DatingApp.API.Shared;
using FluentValidation;

namespace DatingApp.API.Validation
{
    public sealed class UserLoginDtoValidator : AbstractValidator<LoginUserDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("email").Serialize())
                .EmailAddress()
                    .WithMessage(Error.Errors.General.InvalidFormat("email").Serialize());

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("password").Serialize());
        }
    }
}
