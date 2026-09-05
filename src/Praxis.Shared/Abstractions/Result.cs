namespace Praxis.Shared.Abstractions;

/// <summary>Expected failure of a use case. Unexpected failure stays an exception.</summary>
public readonly record struct Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

public class Result
{
    protected Result(bool succeeded, Error error)
    {
        if (succeeded && error != Error.None)
        {
            throw new InvalidOperationException("A successful Result cannot carry an error.");
        }

        if (!succeeded && error == Error.None)
        {
            throw new InvalidOperationException("A failed Result must carry an error.");
        }

        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }

    public bool Failed => !Succeeded;

    public Error Error { get; }

    public static Result Ok() => new(true, Error.None);

    public static Result Fail(Error error) => new(false, error);

    public static Result Fail(string code, string message) => new(false, new Error(code, message));

    public static Result<T> Ok<T>(T value) => new(value, true, Error.None);

    public static Result<T> Fail<T>(Error error) => new(default, false, error);

    public static Result<T> Fail<T>(string code, string message) => new(default, false, new Error(code, message));
}

public class Result<T> : Result
{
    private readonly T? value;

    internal Result(T? value, bool succeeded, Error error) : base(succeeded, error) => this.value = value;

    /// <summary>Only readable when <see cref="Result.Succeeded"/> is true.</summary>
    public T Value => Succeeded
        ? value!
        : throw new InvalidOperationException($"Result failed ({Error.Code}); there is no value to read.");
}
