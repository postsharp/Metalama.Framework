namespace Metalama.Framework.Code;

/// <summary>
/// Defines strategies to compare two instances of the <see cref="IRef{T}"/> interface.
/// </summary>
public enum RefComparison
{
    /// <summary>
    /// Does not support cross-compilation comparisons and ignores nullability when comparing <c>IRef{IType}</c>.
    /// </summary>
    Default,

    /// <summary>
    /// Does not support cross-compilation comparisons and respects nullability when comparing <c>IRef{IType}</c>.
    /// </summary>
    IncludeNullability,

    /// <summary>
    /// Support cross-compilation comparisons and ignores nullability when comparing <c>IRef{IType}</c>.
    /// </summary>
    Structural,

    /// <summary>
    /// Support cross-compilation comparisons and respects nullability when comparing <c>IRef{IType}</c>.
    /// </summary>
    StructuralIncludeNullability,

    /// <summary>
    /// Supports the comparison of string-based references.
    /// </summary>
    Durable
}