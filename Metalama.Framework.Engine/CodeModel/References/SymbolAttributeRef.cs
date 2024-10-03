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
using Attribute = Metalama.Framework.Engine.CodeModel.Source.Attribute;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SymbolAttributeRef : AttributeRef
{
    private readonly AttributeData _attributeData;

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

        this._attributeData = attributeData;
    }

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        if ( !this._attributeData.IsValid() )
        {
            // Only return fully valid attributes.
            attribute = null;

            return false;
        }

        attribute = new Attribute( this._attributeData, compilation, this.ContainingDeclaration.GetTarget( compilation ) );

        return true;
    }

    public override bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey )
    {
        serializationDataKey = this._attributeData;

        return true;
    }

    public override bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData )
    {
        if ( !this._attributeData.IsValid() )
        {
            // Only return fully valid attributes.
            serializationData = null;

            return false;
        }

        serializationData = new AttributeSerializationData(
            this.ContainingDeclaration.GetSymbol( this.CompilationContext.Compilation ).AssertSymbolNotNull(),
            this._attributeData,
            this.CompilationContext );

        return true;
    }

    public override string Name => this._attributeData.AttributeClass?.Name ?? throw new NotSupportedException();

    protected override AttributeSyntax? AttributeSyntax => (AttributeSyntax?) this._attributeData.ApplicationSyntaxReference?.GetSyntax();

    protected override int GetHashCodeCore() => this.AttributeSyntax?.GetHashCode() ?? 0;
}