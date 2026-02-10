using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Common.Validation;

/// Base class for sort field validation - must be inherited by specific modules
public abstract class ValidSortFieldAttribute : ValidationAttribute
{
    protected abstract string[] ValidFields { get; }

    public override bool IsValid(object? value)
    {
        if (value is null || string.IsNullOrEmpty(value.ToString()))
            return true; // Empty is valid (will use default)

        return ValidFields.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"Sort field must be one of: {string.Join(", ", ValidFields)}";
    }
}

/// Generic sort field validation that accepts valid fields via constructor
public class ValidSortFieldGenericAttribute : ValidationAttribute
{
    private readonly string[] _validFields;

    public ValidSortFieldGenericAttribute(params string[] validFields)
    {
        _validFields = validFields ?? throw new ArgumentNullException(nameof(validFields));
    }

    public override bool IsValid(object? value)
    {
        if (value is null || string.IsNullOrEmpty(value.ToString()))
            return true; // Empty is valid (will use default)

        return _validFields.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"Sort field must be one of: {string.Join(", ", _validFields)}";
    }
}
