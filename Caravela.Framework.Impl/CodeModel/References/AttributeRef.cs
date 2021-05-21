// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.References
{
    internal readonly struct AttributeRef : IAttributeRef
    {
        public AttributeRef( AttributeData attributeData, DeclarationRef<IDeclaration> declaringDeclaration )
        {
            this.Target = attributeData;
            this.DeclaringDeclaration = declaringDeclaration;
        }

        public AttributeRef( AttributeBuilder builder )
        {
            this.Target = builder;
            this.DeclaringDeclaration = builder.ContainingDeclaration.ToRef();
        }

        public object? Target { get; }

        public DeclarationRef<INamedType> AttributeType
            => this.Target switch
            {
                AttributeData attributeData => DeclarationRef.FromSymbol<INamedType>( attributeData.AttributeClass.AssertNotNull() ),
                AttributeBuilder reference => reference.Constructor.DeclaringType.ToRef(),
                _ => throw new AssertionFailedException()
            };

        public DeclarationRef<IDeclaration> DeclaringDeclaration { get; }

        public IAttribute Resolve( CompilationModel compilation )
            => this.Target switch
            {
                AttributeData attributeData => new Attribute( attributeData, compilation, this.DeclaringDeclaration.Resolve( compilation ) ),
                AttributeBuilder builder => new BuiltAttribute( builder, compilation ),
                _ => throw new AssertionFailedException()
            };

        public ISymbol GetSymbol( Compilation compilation ) => throw new System.NotSupportedException();

        public override string ToString() => this.Target?.ToString() ?? "null";
    }
}