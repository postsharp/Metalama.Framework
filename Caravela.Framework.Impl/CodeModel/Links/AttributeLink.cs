using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal readonly struct AttributeLink : IAttributeLink
    {
        public AttributeLink( AttributeData attributeData, CodeElementLink<ICodeElement> declaringElement )
        {
            this.LinkedObject = attributeData;
            this.DeclaringElement = declaringElement;
        }

        public AttributeLink( IAttributeLink link )
        {
            this.LinkedObject = link;
            this.DeclaringElement = link.DeclaringElement;
        }

        public object? LinkedObject { get; }

        public CodeElementLink<INamedType> AttributeType
            => this.LinkedObject switch
            {
                AttributeData attributeData => CodeElementLink.FromSymbol<INamedType>(attributeData.AttributeClass.AssertNotNull()),
                IAttributeLink link => link.AttributeType,
                _ => throw new AssertionFailedException()
            };

        public CodeElementLink<ICodeElement> DeclaringElement { get; }

        public IAttribute GetForCompilation( CompilationModel compilation )
            => this.LinkedObject switch
            {
                AttributeData attributeData => new Attribute( attributeData, compilation, this.DeclaringElement.GetForCompilation( compilation ) ),
                IAttributeLink link  => link.GetForCompilation( compilation ),
                _ => throw new AssertionFailedException()
            };
        
        public override string ToString() => this.LinkedObject?.ToString() ?? "null";
    }
}