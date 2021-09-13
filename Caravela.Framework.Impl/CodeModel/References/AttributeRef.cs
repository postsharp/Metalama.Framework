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
            // Find the parent declaration of the custom attribute.
            var declaration = attributeSyntax.Parent?.Parent;

            if ( declaration == null )
            {
                return (null, null);
            }

            ISymbol? parent;

            // Find the parent declaration.
            switch ( declaration )
            {
                case BaseFieldDeclarationSyntax field:
                    {
                        var semanticModel = compilation.GetSemanticModel( attributeSyntax.SyntaxTree );
                        parent = semanticModel.GetDeclaredSymbol( field.Declaration.Variables.First() );

                        break;
                    }

                case MemberDeclarationSyntax or TypeParameterSyntax or ParameterSyntax:
                    {
                        var semanticModel = compilation.GetSemanticModel( attributeSyntax.SyntaxTree );
                        parent = semanticModel.GetDeclaredSymbol( declaration );

                        break;
                    }

                case CompilationUnitSyntax:
                    // This is an assembly-level attribute.
                    parent = compilation.Assembly;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            var attributes = parent?.GetAttributes();

            var attributeData = attributes?.SingleOrDefault(
                a => a.ApplicationSyntaxReference != null && a.ApplicationSyntaxReference.GetSyntax() == attributeSyntax );

            // Save the resolved AttributeData.
            this.Target = attributeData;

            return (attributeData, parent);
        }

        public AttributeRef( AttributeData attributeData, DeclarationRef<IDeclaration> declaringDeclaration )
        {
            this.Target = attributeData;
            this._declaringDeclaration = declaringDeclaration;
        }

        public AttributeRef( AttributeSyntax attributeSyntax )
        {
            this.Target = attributeSyntax;
            this._declaringDeclaration = new DeclarationRef<IDeclaration>( attributeSyntax.Parent! );
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
                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilation.PartialCompilation.Compilation );

                        if ( resolved.Attribute == null || resolved.Parent == null )
                        {
                            return null;
                        }

                        return new Attribute( resolved.Attribute, compilation, compilation.Factory.GetDeclaration( resolved.Parent ) );
                    }

                case AttributeData attributeData:
                    return new Attribute( attributeData, compilation, this._declaringDeclaration.Resolve( compilation ) );

                case AttributeBuilder builder:
                    return new BuiltAttribute( builder, compilation );

                default:
                    throw new AssertionFailedException();
            }
        }

        public ISymbol GetSymbol( Compilation compilation ) => throw new NotSupportedException();

        public override string ToString() => this.Target?.ToString() ?? "null";

        [return: NotNullIfNotNull( "name" )]
        internal static string? GetShortName( string? name ) => name?.TrimEnd( "Attribute" );
    }
}