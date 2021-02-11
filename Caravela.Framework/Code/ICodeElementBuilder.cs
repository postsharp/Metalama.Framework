// unset

namespace Caravela.Framework.Code
{
    public interface ICodeElementBuilder : ICodeElement
    {
        bool IsReadOnly { get; }
        
        IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments );

    }
}