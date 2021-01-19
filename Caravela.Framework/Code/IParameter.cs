namespace Caravela.Framework.Code
{
    public interface IParameter : ICodeElement
    {
        // TODO: should ref-ness be part of the type or the parameter? on parameter
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

        // TODO: default value?
    }
}