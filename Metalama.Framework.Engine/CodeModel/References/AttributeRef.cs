// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal sealed class AttributeRef : IRefImpl<IAttribute>, IEquatable<AttributeRef>
    {
        private readonly Ref<IDeclaration> _containingDeclaration;

        private readonly object _originalTarget;

        public object? Target { get; private set; }

        public Ref<INamedType> AttributeType { get; }

        bool IRefImpl.IsDefault => false;

        public ISymbol GetClosestSymbol( CompilationContext compilation ) => this._containingDeclaration.GetClosestSymbol( compilation );

        DeclarationRefTargetKind IRefImpl.TargetKind => DeclarationRefTargetKind.Default;

        private (AttributeData? Attribute, ISymbol Parent) ResolveAttributeData( AttributeSyntax attributeSyntax, CompilationContext compilation )
        {
            // Find the parent declaration.
            var (attributes, symbol) = this._containingDeclaration.GetAttributeData( compilation );

            // In the parent, find the AttributeData corresponding to the current item.

            var attributeData = attributes.SingleOrDefault(
                a => a.ApplicationSyntaxReference != null && a.ApplicationSyntaxReference.Span == attributeSyntax.Span
                                                          && a.ApplicationSyntaxReference.SyntaxTree == attributeSyntax.SyntaxTree );

            // Save the resolved AttributeData.
            this.Target = attributeData;

            return (attributeData, symbol);
        }

        public AttributeRef( AttributeData attributeData, Ref<IDeclaration> containingDeclaration, CompilationContext compilationContext )
        {
            // Note that Roslyn can return an AttributeData that does not belong to the same compilation
            // as the parent symbol, probably because of some bug or optimisation.

            this.Target = this._originalTarget = attributeData;

            this.AttributeType = Ref.FromSymbol<INamedType>(
                attributeData.AttributeClass.AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes )
                    .TranslateIfNecessary( compilationContext ),
                compilationContext );

            this._containingDeclaration = containingDeclaration;
        }

        public AttributeRef(
            Ref<INamedType> attributeType,
            AttributeSyntax attributeSyntax,
            SyntaxNode? declaration,
            DeclarationRefTargetKind targetKind,
            CompilationContext compilationContext )
        {
            this.AttributeType = attributeType;
            this.Target = this._originalTarget = attributeSyntax;
            this._containingDeclaration = new Ref<IDeclaration>( declaration, targetKind, compilationContext );
        }

        public AttributeRef( in Ref<INamedType> attributeType, AttributeSyntax attributeSyntax, ISymbol declaration, CompilationContext compilationContext )
        {
            this.AttributeType = attributeType;
            this.Target = this._originalTarget = attributeSyntax;
            this._containingDeclaration = Ref.FromSymbol<IDeclaration>( declaration, compilationContext );
        }

        public AttributeRef( AttributeBuilder builder )
        {
            this.AttributeType = builder.Constructor.DeclaringType.ToValueTypedRef();
            this.Target = this._originalTarget = builder;
            this._containingDeclaration = builder.ContainingDeclaration.ToValueTypedRef();
        }

        public AttributeRef( AttributeSerializationData serializationData )
        {
            this.Target = this._originalTarget = serializationData;
            this.AttributeType = serializationData.Type;
            this._containingDeclaration = serializationData.ContainingDeclaration;
        }

        public SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

        public IAttribute GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, out var attribute ) )
            {
                throw new AssertionFailedException( "Attempt to resolve an invalid custom attribute." );
            }

            return attribute;
        }

        public IAttribute? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, out var attribute ) )
            {
                return null;
            }

            return attribute;
        }

        public IRef<TOut> As<TOut>()
            where TOut : class, ICompilationElement
            => this as IRef<TOut> ?? throw new InvalidCastException();

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

                        if (!compilation.CompilationContext.SymbolValidator.IsValid(resolved.Parent))
                        {
                            attribute = null;

                            return false;
                        }

                        attribute = new Attribute(
                            resolved.Attribute,
                            compilation,
                            compilation.Factory.GetDeclaration( resolved.Parent, this._containingDeclaration.TargetKind ) );

                        return true;
                    }

                case AttributeData attributeData:
                    if ( !attributeData.IsValid() )
                    {
                        // Only return fully valid attributes.
                        attribute = null;

                        return false;
                    }

                    attribute = new Attribute( attributeData, compilation, this._containingDeclaration.GetTarget( compilation ) );

                    return true;

                case AttributeBuilder builder:
                    attribute = new BuiltAttribute( builder, compilation );

                    return true;

                case AttributeSerializationData serializationData:
                    attribute = compilation.Factory.GetDeserializedAttribute( serializationData );

                    return true;

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a {this.Target.GetType().Name}.'" );
            }
        }

        public bool TryGetAttributeSerializationDataKey( CompilationContext compilationContext, [NotNullWhen( true )] out object? serializationDataKey )
        {
            switch ( this.Target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.

                    serializationDataKey = null;

                    return false;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilationContext );

                        if ( resolved.Attribute == null || resolved.Parent == null )
                        {
                            serializationDataKey = null;

                            return false;
                        }

                        serializationDataKey = resolved.Attribute;

                        return true;
                    }

                case AttributeData:
                case AttributeBuilder:
                case AttributeSerializationData:
                    serializationDataKey = this.Target;

                    return true;

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a {this.Target.GetType().Name}.'" );
            }
        }

        public bool TryGetAttributeSerializationData(
            CompilationContext compilationContext,
            [NotNullWhen( true )] out AttributeSerializationData? serializationData )
        {
            switch ( this.Target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.

                    serializationData = null;

                    return false;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilationContext );

                        if ( resolved.Attribute == null || resolved.Parent == null )
                        {
                            serializationData = null;

                            return false;
                        }

                        serializationData = new AttributeSerializationData( resolved.Parent, resolved.Attribute, compilationContext );

                        return true;
                    }

                case AttributeData attributeData:
                    if ( !attributeData.IsValid() )
                    {
                        // Only return fully valid attributes.
                        serializationData = null;

                        return false;
                    }

                    serializationData = new AttributeSerializationData(
                        this._containingDeclaration.GetSymbol( compilationContext.Compilation ).AssertSymbolNotNull(),
                        attributeData,
                        compilationContext );

                    return true;

                case AttributeBuilder builder:
                    serializationData = new AttributeSerializationData( builder );

                    return true;

                case AttributeSerializationData mySerializationData:
                    serializationData = mySerializationData;

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

            if ( !RefEqualityComparer<IDeclaration>.Default.Equals( this._containingDeclaration, other._containingDeclaration ) )
            {
                return false;
            }

            return true;
        }

        bool IEquatable<IRef<ICompilationElement>>.Equals( IRef<ICompilationElement>? other )
            => other is AttributeRef otherAttributeRef && this.Equals( otherAttributeRef );

        bool IRef<IAttribute>.Equals( IRef<ICompilationElement>? other, bool includeNullability )
            => other is AttributeRef otherAttributeRef && this.Equals( otherAttributeRef );

        public override int GetHashCode()
        {
            var targetHashCode = this._originalTarget switch
            {
                AttributeSyntax syntax => syntax.GetHashCode(),
                AttributeData data => data.ApplicationSyntaxReference?.GetSyntax().GetHashCode() ?? 0,
                AttributeBuilder builder => builder.GetHashCode(),
                _ => throw new AssertionFailedException( $"Unexpected target type '{this._originalTarget.GetType()}'." )
            };

            return HashCode.Combine( targetHashCode, RefEqualityComparer<IDeclaration>.Default.GetHashCode( this._containingDeclaration ) );
        }
    }
}