#pragma warning disable SA1623 // Property summary documentation should match accessors

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a parameter of a method or property.
    /// </summary>
    public interface IParameter : ICodeElement
    {
        /// <summary>
        /// Gets the <c>in</c>, <c>out</c>, <c>ref</c> parameter type modifier.
        /// </summary>
        RefKind RefKind { get; }

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c>, <c>out</c> and <c>in</c> parameters and <c>ref</c> and <c>ref readonly</c> return parameters.
        /// </summary>
        bool IsByRef { get; }

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c> parameters.
        /// </summary>
        bool IsRef { get; }

        /// <summary>
        /// Returns <c>true</c> for <c>out</c> parameters.
        /// </summary>
        bool IsOut { get; }

        bool IsParams { get; }

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        IType Type { get; }

        /// <summary>
        /// Gets the parameter type, or <c>null</c> for <see cref="IMethod.ReturnParameter"/>.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Gets the parameter position, or <c>-1</c> for <see cref="IMethod.ReturnParameter"/>.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter has a default value.
        /// </summary>
        bool HasDefaultValue { get; }

        /// <remarks>
        /// Gets the default value of the parameter, or  <c>null</c> if the parameter type is a struct and the default
        /// value of the parameter is the default value of the struct type.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
        object? DefaultValue { get; }
    }
}