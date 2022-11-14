namespace Metalama.Framework.Introspection;

/// <summary>
/// Represents a fabric.
/// </summary>
public interface IIntrospectionFabric : IIntrospectionAspectPredecessorInternal
{
    /// <summary>
    /// Gets the full name of the fabric type.
    /// </summary>
    string FullName { get; }
}