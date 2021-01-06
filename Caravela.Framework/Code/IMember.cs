namespace Caravela.Framework.Code
{
    public interface IMember : ICodeElement
    {
        string Name { get; }

        bool IsStatic { get; }

        bool IsVirtual { get; }

        INamedType? DeclaringType { get; }
    }
}