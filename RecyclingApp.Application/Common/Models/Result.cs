using System.Collections.Generic;

namespace RecyclingApp.Application.Common.Models;

/// <summary>
/// Non-generic outcome result contract.
/// </summary>
public class Result
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result Success(string? message = null) => new()
    {
        Succeeded = true,
        Message = message
    };

    public static Result Failure(string message, IEnumerable<string>? errors = null)
    {
        var result = new Result
        {
            Succeeded = false,
            Message = message
        };

        if (errors != null)
        {
            result.Errors.AddRange(errors);
        }

        return result;
    }
}

/// <summary>
/// Generic outcome result contract wrapping payload data.
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public class Result<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result<T> Success(T data, string? message = null) => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static Result<T> Failure(string message, IEnumerable<string>? errors = null)
    {
        var result = new Result<T>
        {
            Succeeded = false,
            Data = default,
            Message = message
        };

        if (errors != null)
        {
            result.Errors.AddRange(errors);
        }

        return result;
    }
}
