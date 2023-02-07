// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal sealed class AttributeRef : IRefImpl<IAttribute>, IEquatable<AttributeRef>
    {
        private readonly Ref<IDeclaration> _declaringDeclaration;

        private readonly object _originalTarget;

        public object? Target { get; private set; }

        bool IRefImpl.IsDefault => false;

        public ISymbol GetClosestSymbol( CompilationContext compilation ) => this._declaringDeclaration.GetSymbol( compilation );

        private (AttributeData? Attribute, ISymbol? Parent) ResolveAttributeData( AttributeSyntax attributeSyntax, CompilationContext compilation )
        {
            // Find the parent declaration.
            var resolved =
                this._declaringDeclaration.GetAttributeData( compilation );

            // In the parent, find the AttributeData corresponding to the current item.

            var attributeData = resolved.Attributes.SingleOrDefault(
                a => a.ApplicationSyntaxReference != null && a.ApplicationSyntaxReference.Span == attributeSyntax.Span
                                                          && a.ApplicationSyntaxReference.SyntaxTree == attributeSyntax.SyntaxTree );

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
            this.Target = this._originalTarget = attributeData;
            this._declaringDeclaration = declaringDeclaration;
        }

        public AttributeRef(
            AttributeSyntax attributeSyntax,
            SyntaxNode? declaration,
            DeclarationRefTargetKind targetKind,
            CompilationContext compilationContext )
        {
            this.Target = this._originalTarget = attributeSyntax;
            this._declaringDeclaration = new Ref<IDeclaration>( declaration, targetKind, compilationContext );
        }

        public AttributeRef( AttributeSyntax attributeSyntax, in Ref<IDeclaration> declaration )
        {
            this.Target = this._originalTarget = attributeSyntax;
            this._declaringDeclaration = declaration;
        }

        public AttributeRef( AttributeBuilder builder )
        {
            this.Target = this._originalTarget = builder;
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
                        _ => throw new AssertionFailedException(
                            $"Unexpected attribute name syntax: {attributeSyntax.Name.Kind()} at '{attributeSyntax.GetLocation()}'." )
                    },
                    _ => throw new AssertionFailedException( $"Unexpected target type '{this.Target?.GetType()}'." )
                } );

        public SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

        public IAttribute GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, out var attribute ) )
            {
                throw new AssertionFailedException( "Attempt to resolve an invalid custom attribute." );
            }

            return attribute;
        }

        private AttributeSyntax? Syntax
            => this.Target switch
            {
                null => null,
                AttributeSyntax syntax => syntax,
                AttributeData data => (AttributeSyntax?) data.ApplicationSyntaxReference?.GetSyntax(),
                AttributeBuilder => null,
                _ => throw new AssertionFailedException( $"Unexpected target type '{this.Target.GetType()}'." )
            };

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
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilation.CompilationContext );

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

        ISymbol ISdkRef<IAttribute>.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();

        public override string ToString() => this.Target?.ToString() ?? "null";

        public bool IsSyntax( AttributeSyntax attribute )
            => this.Target switch
            {
                AttributeData targetAttributeData => targetAttributeData.ApplicationSyntaxReference?.GetSyntax() == attribute,
                AttributeSyntax targetAttributeSyntax => targetAttributeSyntax == attribute,
                _ => false
            };

        public bool Equals( AttributeRef? other )
        {
            if ( other == null )
            {
                return false;
            }

            switch ( this._originalTarget )
            {
                case AttributeSyntax syntax:
                    if ( syntax != other.Syntax )
                    {
                        return false;
                    }

                    break;

                case AttributeData data:
                    if ( data.ApplicationSyntaxReference?.GetSyntax() != other.Syntax )
                    {
                        return false;
                    }

                    break;

                case AttributeBuilder builder:
                    if ( builder != other.Target )
                    {
                        return false;
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected target type '{this._originalTarget.GetType()}'." );
            }

            if ( !RefEqualityComparer<IDeclaration>.Default.Equals( this._declaringDeclaration, other._declaringDeclaration ) )
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var targetHashCode = this._originalTarget switch
            {
                AttributeSyntax syntax => syntax.GetHashCode(),
                AttributeData data => data.ApplicationSyntaxReference?.GetSyntax().GetHashCode() ?? 0,
                AttributeBuilder builder => builder.GetHashCode(),
                _ => throw new AssertionFailedException( $"Unexpected target type '{this._originalTarget.GetType()}'." )
            };

            return HashCode.Combine( targetHashCode, RefEqualityComparer<IDeclaration>.Default.GetHashCode( this._declaringDeclaration ) );
        }
    }
}