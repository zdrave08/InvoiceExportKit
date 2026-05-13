namespace InvoiceExportKit.Abstractions.Results;

/// <summary>Outcome of validating an <see cref="Models.InvoiceModel"/>.</summary>
public sealed class ValidationResult
{
    private readonly List<string> _errors = new();

    /// <summary>Creates a successful (no-error) result.</summary>
    public static ValidationResult Success() => new();

    /// <summary>Creates a failed result pre-loaded with <paramref name="errors"/>.</summary>
    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        var result = new ValidationResult();
        result._errors.AddRange(errors);
        return result;
    }

    /// <summary><c>true</c> when no validation errors were recorded.</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>Read-only list of human-readable error messages.</summary>
    public IReadOnlyList<string> Errors => _errors;

    /// <summary>Adds a single error message and returns this instance for fluent chaining.</summary>
    public ValidationResult AddError(string message)
    {
        _errors.Add(message);
        return this;
    }
}
