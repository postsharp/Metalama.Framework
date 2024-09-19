// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class SymbolAttributeRef : AttributeRef
{
    public AttributeData AttributeData { get; }

    public SymbolAttributeRef( AttributeData attributeData, IRef<IDeclaration> containingDeclaration, CompilationContext compilationContext )
        : base(
            containingDeclaration,
            compilationContext.RefFactory.FromSymbol<INamedType>(
                attributeData.AttributeClass.AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes )
                    .TranslateIfNecessary( compilationContext ) ),
            compilationContext )
    {
        // Note that Roslyn can return an AttributeData that does not belong to the same compilation
        // as the parent symbol, probably because of some bug or optimisation.

        this.AttributeData = attributeData;
    }

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        if ( !this.AttributeData.IsValid() )
        {
            // Only return fully valid attributes.
            attribute = null;

            return false;
        }

        attribute = new Attribute( this.AttributeData, compilation, this.ContainingDeclaration.GetTarget( compilation ) );

        return true;
    }

    public override bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey )
    {
        serializationDataKey = this.AttributeData;

        return true;
    }

    public override bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData )
    {
        if ( !this.AttributeData.IsValid() )
        {
            // Only return fully valid attributes.
            serializationData = null;

            return false;
        }

        serializationData = new AttributeSerializationData(
            this.ContainingDeclaration.GetSymbol( this.CompilationContext.Compilation ).AssertSymbolNotNull(),
            this.AttributeData,
            this.CompilationContext );

        return true;
    }

    protected override AttributeSyntax? AttributeSyntax => (AttributeSyntax?) this.AttributeData.ApplicationSyntaxReference?.GetSyntax();

    protected override int GetHashCodeCore() => this.AttributeSyntax?.GetHashCode() ?? 0;

    public override bool Equals( AttributeRef? other ) => throw new NotImplementedException();
}