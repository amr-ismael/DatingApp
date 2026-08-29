using System;

namespace DatingApp.API.Shared
{
    /// <summary>
    /// A success or a failure, without exceptions for expected outcomes.
    /// </summary>
    public class Result
    {
        public bool IsSuccessful { get; }
        public bool IsFailure => !IsSuccessful;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException("A successful result cannot carry an error.");
            }

            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException("A failed result must carry an error.");
            }

            IsSuccessful = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, Error.None);
        public static Result<T> Success<T>(T value) => new Result<T>(value, true, Error.None);
        public static Result Failure(Error error) => new Result(false, error);
        public static Result<T> Failure<T>(Error error) => new Result<T>(false, error);
    }

    public class Result<T> : Result
    {
        private readonly T _value;

        protected internal Result(bool isSuccess, Error error) : base(isSuccess, error)
        {
        }

        protected internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
            => _value = value;

        public T Value => IsSuccessful
            ? _value
            : throw new InvalidOperationException("The value of a failed result cannot be accessed.");
    }
}
