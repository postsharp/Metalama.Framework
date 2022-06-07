// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal class AttributeRef : IRefImpl<IAttribute>
    {
        private readonly Ref<IDeclaration> _declaringDeclaration;

        public object? Target { get; private set; }

        bool IRefImpl.IsDefault => false;

        private (AttributeData? Attribute, ISymbol? Parent) ResolveAttributeData( AttributeSyntax attributeSyntax, Compilation compilation )
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

        public AttributeRef( AttributeData attributeData, Ref<IDeclaration> declaringDeclaration )
        {
            this.Target = attributeData;
            this._declaringDeclaration = declaringDeclaration;
        }

        public AttributeRef( AttributeSyntax attributeSyntax, SyntaxNode? declaration, DeclarationRefTargetKind targetKind, Compilation compilation )
        {
            this.Target = attributeSyntax;
            this._declaringDeclaration = new Ref<IDeclaration>( declaration, targetKind, compilation );
        }

        public AttributeRef( AttributeSyntax attributeSyntax, in Ref<IDeclaration> declaration )
        {
            this.Target = attributeSyntax;
            this._declaringDeclaration = declaration;
        }

        public AttributeRef( AttributeBuilder builder )
        {
            this.Target = builder;
            this._declaringDeclaration = builder.ContainingDeclaration.ToTypedRef();
        }

        public string? AttributeTypeName
            => AttributeHelper.GetShortName(
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

        public DeclarationSerializableId ToSerializableId() => throw new NotSupportedException();

        public IAttribute GetTarget( ICompilation compilation )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, out var attribute ) )
            {
                throw new AssertionFailedException( "Attempt to resolve an invalid custom attribute." );
            }

            return attribute;
        }

        public bool TryGetTarget( CompilationModel compilation, [NotNullWhen( true )] out IAttribute? attribute )
        {
            switch ( this.Target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.
                    attribute = null;

                    return false;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilation.PartialCompilation.Compilation );

                        if ( resolved.Attribute == null || resolved.Parent == null )
                        {
                            attribute = null;

                            return false;
                        }

                        attribute = new Attribute(
                            resolved.Attribute,
                            compilation,
                            compilation.Factory.GetDeclaration( resolved.Parent, this._declaringDeclaration.TargetKind ) );

                        return true;
                    }

                case AttributeData attributeData:
                    attribute = new Attribute( attributeData, compilation, this._declaringDeclaration.GetTarget( compilation ) );

                    return true;

                case AttributeBuilder builder:
                    attribute = new BuiltAttribute( builder, compilation );

                    return true;

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a {this.Target.GetType().Name}.'" );
            }
        }

        ISymbol? ISdkRef<IAttribute>.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();

        public override string ToString() => this.Target?.ToString() ?? "null";

        public bool IsSyntax( AttributeSyntax attribute )
            => this.Target switch
            {
                null => false,
                AttributeData targetAttributeData => targetAttributeData.ApplicationSyntaxReference?.GetSyntax() == attribute,
                AttributeSyntax targetAttributeSyntax => targetAttributeSyntax == attribute,
                _ => throw new AssertionFailedException()
            };
    }
}