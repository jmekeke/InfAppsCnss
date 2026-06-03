namespace Cnss.Metier.Shared.Domain.Common;

/// <summary>
/// Résultat générique pour les use cases. Remplace les tuples (bool, string?).
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public int? StatusCode { get; }

    protected Result(bool isSuccess, string? error, int? statusCode = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result NotFound(string error = "Ressource introuvable.") => new(false, error, 404);
    public static Result Conflict(string error) => new(false, error, 409);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error, int statusCode = 400) => new(default, false, error, statusCode);
    public static Result<T> NotFound<T>(string error = "Ressource introuvable.") => new(default, false, error, 404);
    public static Result<T> Conflict<T>(string error) => new(default, false, error, 409);
}

/// <summary>
/// Résultat typé avec valeur de retour.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error, int? statusCode = null)
        : base(isSuccess, error, statusCode)
    {
        Value = value;
    }
}
