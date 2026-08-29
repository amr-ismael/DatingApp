using DatingApp.API.Dtos;
using DatingApp.API.Shared;
using FluentValidation;

namespace DatingApp.API.Validation
{
    public sealed class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("username").Serialize());

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("password").Serialize());
        }
    }
}
