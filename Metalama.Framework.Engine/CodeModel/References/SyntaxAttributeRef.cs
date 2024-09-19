// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class SyntaxAttributeRef : AttributeRef
{
    private readonly AttributeSyntax _attributeSyntax;

    public SyntaxAttributeRef(
        IRef<INamedType> attributeType,
        AttributeSyntax attributeSyntax,
        SyntaxNode declaration,
        CompilationContext compilationContext,
        DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default ) : base(
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
        symbol.ToRef<IDeclaration>( compilationContext ),
        attributeType,
        compilationContext )
    {
        this._attributeSyntax = attributeSyntax;
    }
    
    private ResolvedRef? _resolvedRef;

    protected record ResolvedRef( AttributeData AttributeData, ISymbol Parent )
    {
        public static ResolvedRef Invalid { get; } = new( null!, null! );
    }

    protected ResolvedRef? ResolveAttributeData( AttributeSyntax attributeSyntax )
    {
        if ( this._resolvedRef != null )
        {
            if ( this._resolvedRef == ResolvedRef.Invalid )
            {
                return null;
            }
            else
            {
                return this._resolvedRef;
            }
        }

        // Find the parent declaration.
        var (attributes, symbol) = this.ContainingDeclaration.Unwrap().GetAttributeData();

        // In the parent, find the AttributeData corresponding to the current item.

        var attributeData = attributes.SingleOrDefault(
            a => a.ApplicationSyntaxReference != null && a.ApplicationSyntaxReference.Span == attributeSyntax.Span
                                                      && a.ApplicationSyntaxReference.SyntaxTree == attributeSyntax.SyntaxTree );

        if ( attributeData != null )
        {
            // Save the resolved AttributeData.
            return this._resolvedRef = new ResolvedRef( attributeData, symbol );
        }
        else
        {
            this._resolvedRef = ResolvedRef.Invalid;

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
            resolved.AttributeData,
            compilation,
            this.ContainingDeclaration.GetTarget( compilation, genericContext: genericContext ) );

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

        serializationData = new AttributeSerializationData( resolved.Parent, resolved.AttributeData, this.CompilationContext );

        return true;
    }

    protected override AttributeSyntax? AttributeSyntax => this._attributeSyntax;

    protected override int GetHashCodeCore() => this._attributeSyntax.GetHashCode();
}