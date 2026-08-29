using System;

namespace DatingApp.API.Shared
{
    /// <summary>
    /// A failure with a machine-readable code, a human message, and the field that caused it.
    ///
    /// Validators carry these through FluentValidation's message string via
    /// <see cref="Serialize"/>; <see cref="ModelStateValidator"/> unpacks them again on the
    /// way out. The code is what a client switches on — the message is what it falls back to.
    /// </summary>
    public sealed class Error
    {
        public static readonly Error None = new Error(string.Empty, string.Empty);
        public static readonly Error NullValue = new Error("error.null.value", "Result value is null");

        public static class Errors
        {
            public static class General
            {
                public static Error IsRequiredError(string field)
                    => new Error($"{field}.is.required", $"{field} is required", field);
                public static Error MinLengthError(int minLength, string field)
                    => new Error("min.length.error", $"minimum length is {minLength}", field);
                public static Error MaxLengthError(int maxLength, string field)
                    => new Error("max.length.error", $"maximum length is {maxLength}", field);
                public static Error InvalidFieldDataType(string field)
                    => new Error("invalid.input.data", $"Input data error in field '{field}'", field);
                public static Error ValidationError(string message)
                    => new Error("validation.error", message, "validation");
                public static Error InternalServiceError()
                    => new Error("internal.service.error", "an unexpected error occurred", "internal");
            }

            public static class Auth
            {
                public static Error UsernameTaken()
                    => new Error("auth.username.taken", "username is already taken", "username");
                public static Error InvalidCredentials()
                    => new Error("auth.invalid.credentials", "invalid username or password", "credentials");
            }

            public static class Users
            {
                public static Error NotFound()
                    => new Error("user.not.found", "user not found", "id");
            }

            public static class Matches
            {
                public static Error NotFound()
                    => new Error("match.not.found", "match not found", "id");
                public static Error NotAuthorized()
                    => new Error("match.not.authorized", "you are not part of this match", "id");
            }
        }

        public string Code { get; }
        public string Message { get; }
        public string InvalidField { get; }

        private const string Sep = "||";

        public string Serialize() => $"{Code}{Sep}{Message}{Sep}{InvalidField}";

        public static Error Deserialize(string serialized)
        {
            var data = serialized.Split(new[] { Sep }, StringSplitOptions.RemoveEmptyEntries);

            return data.Length < 3 ? null : new Error(data[0], data[1], data[2]);
        }

        public Error(string code, string message) : this(code, message, "N/A")
        {
        }

        public Error(string code, string message, string invalidField)
        {
            Code = code;
            Message = message;
            InvalidField = invalidField;
        }

        public static implicit operator string(Error error) => error.Code;
    }
}
