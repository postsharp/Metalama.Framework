// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.References
{
    internal class AttributeRef : IDeclarationRef<IAttribute>
    {
        private readonly DeclarationRef<IDeclaration> _declaringDeclaration;

        public object? Target { get; private set; }

        private ( AttributeData? Attribute, ISymbol? Parent ) ResolveAttributeData( AttributeSyntax attributeSyntax, Compilation compilation )
        {
            // Find the parent declaration.
            var resolved =
                this._declaringDeclaration.GetAttributeData( compilation );

            // In the parent, find the AttributeData corresponding to the current item.

            var attributeData = resolved.Attributes.SingleOrDefault(
                a => a.ApplicationSyntaxReference != null && a.ApplicationSyntaxReference.Span == attributeSyntax.Span );

            if ( attributeData == null )
            {
                // This should not happen in a valid compilation and it's a good place to add a breakpoint.
            }

            // Save the resolved AttributeData.
            this.Target = attributeData;

            return (attributeData, resolved.Symbol);
        }

        public AttributeRef( AttributeData attributeData, DeclarationRef<IDeclaration> declaringDeclaration )
        {
            this.Target = attributeData;
            this._declaringDeclaration = declaringDeclaration;
        }

        public AttributeRef( AttributeSyntax attributeSyntax, SyntaxNode? declaration, DeclarationRefTargetKind targetKind, Compilation compilation )
        {
            this.Target = attributeSyntax;
            this._declaringDeclaration = new DeclarationRef<IDeclaration>( declaration, targetKind, compilation );
        }

        public AttributeRef( AttributeBuilder builder )
        {
            this.Target = builder;
            this._declaringDeclaration = builder.ContainingDeclaration.ToRef();
        }

        public string? AttributeTypeName
            => GetShortName(
                this.Target switch
                {
                    AttributeData attributeData => attributeData.AttributeClass?.Name,
                    AttributeBuilder reference => reference.Constructor.DeclaringType.Name,
                    AttributeSyntax attributeSyntax => attributeSyntax.Name switch
                    {
                        SimpleNameSyntax simpleName => simpleName.Identifier.Text,
                        QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
                        _ => throw new AssertionFailedException()
                    },
                    _ => throw new AssertionFailedException()
                } );

        public IAttribute? Resolve( CompilationModel compilation )
        {
            switch ( this.Target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.
                    return null;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilation.PartialCompilation.Compilation );

                        if ( resolved.Attribute == null || resolved.Parent == null )
                        {
                            return null;
                        }

                        return new Attribute(
                            resolved.Attribute,
                            compilation,
                            compilation.Factory.GetDeclaration( resolved.Parent, this._declaringDeclaration.TargetKind ) );
                    }

                case AttributeData attributeData:
                    return new Attribute( attributeData, compilation, this._declaringDeclaration.Resolve( compilation ) );

                case AttributeBuilder builder:
                    return new BuiltAttribute( builder, compilation );

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a {this.Target.GetType().Name}.'" );
            }
        }

        public ISymbol GetSymbol( Compilation compilation ) => throw new NotSupportedException();

        public override string ToString() => this.Target?.ToString() ?? "null";

        [return: NotNullIfNotNull( "name" )]
        internal static string? GetShortName( string? name ) => name?.TrimEnd( "Attribute" );
    }
}