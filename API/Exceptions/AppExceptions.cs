namespace VehicleProfitTracker.API.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenAppException : Exception
{
    public ForbiddenAppException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message) { }
}

/// <summary>
/// Raised when a request fails business/input validation.
/// Named ValidationAppException to avoid clashing with System.ComponentModel's ValidationException.
/// </summary>
public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationAppException(string field, string error)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}
