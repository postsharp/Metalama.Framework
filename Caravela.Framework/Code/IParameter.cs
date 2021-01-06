namespace Caravela.Framework.Code
{
    public interface IParameter : ICodeElement
    {
        // TODO: should ref-ness be part of the type or the parameter? on parameter
        bool IsOut { get; }

        IType Type { get; }

        /// <remarks><see langword="null"/> for <see cref="IMethod.ReturnParameter"/></remarks>
        string? Name { get; }

        /// <remarks>-1 for <see cref="IMethod.ReturnParameter"/></remarks>
        int Index { get; }

        // TODO: default value?
    }
}