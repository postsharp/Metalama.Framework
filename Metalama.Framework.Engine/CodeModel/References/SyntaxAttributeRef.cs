// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SyntaxAttributeRef : AttributeRef
{
    private readonly AttributeSyntax _attributeSyntax;

    public SyntaxAttributeRef(
        IRef<INamedType> attributeType,
        AttributeSyntax attributeSyntax,
        SyntaxNode declaration,
        CompilationContext compilationContext,
        RefTargetKind targetKind = RefTargetKind.Default ) : base(
        new SyntaxRef<IDeclaration>( declaration, targetKind, compilationContext ),
        attributeType,
        compilationContext )
    {
        this._attributeSyntax = attributeSyntax;
    }

    public SyntaxAttributeRef(
        IRef<INamedType> attributeType,
        AttributeSyntax attributeSyntax,
        ISymbol symbol,
        CompilationContext compilationContext ) : base(
        symbol.ToRef( compilationContext ),
        attributeType,
        compilationContext )
    {
        this._attributeSyntax = attributeSyntax;
    }

    private ResolvedAttributeRef? _resolvedRef;

    private ResolvedAttributeRef? ResolveAttributeData( AttributeSyntax attributeSyntax )
    {
        if ( this._resolvedRef != null )
        {
            if ( this._resolvedRef == ResolvedAttributeRef.Invalid )
            {
                return null;
            }
            else
            {
                return this._resolvedRef;
            }
        }

        // Find the parent declaration.
        var resolved = ((ICompilationBoundRefImpl) this.ContainingDeclaration).GetAttributeData();

        // In the parent, find the AttributeData corresponding to the current item.

        var attributeData = resolved.Attributes.SingleOrDefault(
            a => a.ApplicationSyntaxReference != null && a.ApplicationSyntaxReference.Span == attributeSyntax.Span
                                                      && a.ApplicationSyntaxReference.SyntaxTree == attributeSyntax.SyntaxTree );

        if ( attributeData != null )
        {
            if ( resolved.Attributes.Length != 1 )
            {
                resolved = resolved with { Attributes = ImmutableArray.Create( attributeData ) };
            }

            // Save the resolved AttributeData.
            return this._resolvedRef = resolved;
        }
        else
        {
            this._resolvedRef = ResolvedAttributeRef.Invalid;

            return null;
        }
    }

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        var resolved = this.ResolveAttributeData( this._attributeSyntax );

        if ( resolved == null )
        {
            attribute = null;

            return false;
        }

        attribute = new Attribute(
            resolved.Attributes[0],
            compilation,
            compilation.Factory.GetDeclaration( resolved.ParentSymbol, resolved.ParentRefTargetKind ) );

        return true;
    }

    public override bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey )
    {
        serializationDataKey = this._attributeSyntax;

        return true;
    }

    public override bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData )
    {
        var resolved = this.ResolveAttributeData( this._attributeSyntax );

        if ( resolved == null )
        {
            serializationData = null;

            return false;
        }

        serializationData = new AttributeSerializationData( resolved.ParentSymbol, resolved.Attributes[0], this.CompilationContext );

        return true;
    }

    public override string Name => throw new NotSupportedException();

    protected override AttributeSyntax? AttributeSyntax => this._attributeSyntax;

    protected override int GetHashCodeCore() => this._attributeSyntax.GetHashCode();
}