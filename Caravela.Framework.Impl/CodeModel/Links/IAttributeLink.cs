using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal interface IAttributeLink : ICodeElementLink<IAttribute>
    {
        // Intentionally using the struct and not the interface to avoid memory allocation.
        CodeElementLink<INamedType> AttributeType { get; }
        CodeElementLink<ICodeElement> DeclaringElement { get; }
    }
}