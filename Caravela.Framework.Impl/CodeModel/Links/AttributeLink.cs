// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
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

        public AttributeLink( AttributeBuilder builder )
        {
            this.Target = builder;
            this.DeclaringElement = builder.ContainingElement.ToLink();
        }

        public object? Target { get; }

        public CodeElementLink<INamedType> AttributeType
            => this.Target switch
            {
                AttributeData attributeData => CodeElementLink.FromSymbol<INamedType>( attributeData.AttributeClass.AssertNotNull() ),
                AttributeBuilder link => link.Constructor.DeclaringType.ToLink(),
                _ => throw new AssertionFailedException()
            };

        public CodeElementLink<ICodeElement> DeclaringElement { get; }

        public IAttribute GetForCompilation( CompilationModel compilation )
            => this.Target switch
            {
                AttributeData attributeData => new Attribute( attributeData, compilation, this.DeclaringElement.GetForCompilation( compilation ) ),
                AttributeBuilder builder => new BuiltAttribute( builder, compilation ),
                _ => throw new AssertionFailedException()
            };

        public override string ToString() => this.Target?.ToString() ?? "null";
    }
}