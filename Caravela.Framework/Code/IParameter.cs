namespace Caravela.Framework.Code
{
    public interface IParameter : ICodeElement
    {
        RefKind RefKind { get; }

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c>, <c>out</c> and <c>in</c> parameters and <c>ref</c> and <c>ref readonly</c> return parameters.
        /// </summary>
        bool IsByRef { get; }

        bool IsRef { get; }

        bool IsOut { get; }

        IType Type { get; }

        /// <remarks><see langword="null"/> for <see cref="IMethod.ReturnParameter"/></remarks>
        string? Name { get; }

        /// <remarks>-1 for <see cref="IMethod.ReturnParameter"/></remarks>
        int Index { get; }

        bool HasDefaultValue { get; }

        /// <remarks>
        /// Returns <c>null</c> if the parameter type is a struct and the default value of the parameter is the default value of the struct type.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
        object? DefaultValue { get; }
    }
}