﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
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
    private readonly IFullRef<IDeclaration> _containingDeclaration;
    private readonly RefFactory _refFactory;

    public SymbolAttributeRef( AttributeData attributeData, IFullRef<IDeclaration> containingDeclaration, RefFactory refFactory )
    {
        // Note that Roslyn can return an AttributeData that does not belong to the same compilation
        // as the parent symbol, probably because of some bug or optimisation.

        this._attributeData = attributeData;
        this._containingDeclaration = containingDeclaration;
        this._refFactory = refFactory;
    }

    public override IRef<IDeclaration> ContainingDeclaration => this._containingDeclaration;

    public override IRef<INamedType> AttributeType
        => this._refFactory.FromSymbol<INamedType>(
            this._attributeData.AttributeClass.AssertSymbolNotNull()
                .TranslateIfNecessary( this._refFactory.CompilationContext ) );

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
            this.ContainingDeclaration.GetSymbol( this._refFactory.CompilationContext.Compilation ).AssertSymbolNotNull(),
            this._attributeData,
            this._refFactory );

        return true;
    }

    public override string Name => this._attributeData.AttributeClass?.Name ?? throw new NotSupportedException();

    protected override AttributeSyntax? AttributeSyntax => (AttributeSyntax?) this._attributeData.ApplicationSyntaxReference?.GetSyntax();

    protected override int GetHashCodeCore() => this.AttributeSyntax?.GetHashCode() ?? 0;
}