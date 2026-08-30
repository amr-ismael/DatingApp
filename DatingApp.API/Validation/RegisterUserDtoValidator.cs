using System;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
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
        private const int MinimumAge = 18;

        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("firstName").Serialize());

            RuleFor(x => x.LastName)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("lastName").Serialize());

            RuleFor(x => x.DateOfBirth)
                .NotEqual(default(DateTime))
                    .WithMessage(Error.Errors.General.IsRequiredError("dateOfBirth").Serialize())
                .Must(dob => dob.CalculateAge() >= MinimumAge)
                    .WithMessage(Error.Errors.General.ValidationError($"must be at least {MinimumAge} years old").Serialize())
                    .When(x => x.DateOfBirth != default);

            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("email").Serialize())
                .EmailAddress()
                    .WithMessage(Error.Errors.General.InvalidFormat("email").Serialize());

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("password").Serialize())
                .MinimumLength(8)
                    .WithMessage(Error.Errors.General.MinLengthError(8, "password").Serialize())
                .MaximumLength(64)
                    .WithMessage(Error.Errors.General.MaxLengthError(64, "password").Serialize());

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                    .WithMessage(Error.Errors.General.IsRequiredError("confirmPassword").Serialize())
                .Equal(x => x.Password)
                    .WithMessage(Error.Errors.General.ValidationError("passwords do not match").Serialize());

            RuleFor(x => x.Gender)
                .NotNull()
                    .WithMessage(Error.Errors.General.IsRequiredError("gender").Serialize())
                .IsInEnum()
                    .WithMessage(Error.Errors.General.InvalidFormat("gender").Serialize());

            RuleFor(x => x.InterestedIn)
                .NotNull()
                    .WithMessage(Error.Errors.General.IsRequiredError("interestedIn").Serialize())
                .IsInEnum()
                    .WithMessage(Error.Errors.General.InvalidFormat("interestedIn").Serialize());
        }
    }
}
