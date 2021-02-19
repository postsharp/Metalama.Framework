using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal readonly struct AttributeLink : IAttributeLink
    {
        public AttributeLink( AttributeData attributeData, CodeElementLink<ICodeElement> declaringElement )
        {
            this.Target = attributeData;
            this.DeclaringElement = declaringElement;
        }

        public AttributeLink( IAttributeLink link )
        {
            this.Target = link;
            this.DeclaringElement = link.DeclaringElement;
        }

        public object? Target { get; }

        public CodeElementLink<INamedType> AttributeType
            => this.Target switch
            {
                AttributeData attributeData => CodeElementLink.FromSymbol<INamedType>(attributeData.AttributeClass.AssertNotNull()),
                IAttributeLink link => link.AttributeType,
                _ => throw new AssertionFailedException()
            };

        public CodeElementLink<ICodeElement> DeclaringElement { get; }

        public IAttribute GetForCompilation( CompilationModel compilation )
            => this.Target switch
            {
                AttributeData attributeData => new Attribute( attributeData, compilation, this.DeclaringElement.GetForCompilation( compilation ) ),
                IAttributeLink link  => link.GetForCompilation( compilation ),
                _ => throw new AssertionFailedException()
            };
        
        public override string ToString() => this.Target?.ToString() ?? "null";
    }
}