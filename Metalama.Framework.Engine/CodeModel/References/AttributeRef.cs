// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal sealed class AttributeRef : IRefImpl<IAttribute>, IEquatable<AttributeRef>
    {
        private readonly IRef<IDeclaration> _containingDeclaration;

        private readonly object _target;
        private ResolvedRef? _resolvedRef;

        private record ResolvedRef( AttributeData AttributeData, ISymbol Parent )
        {
            public static ResolvedRef Invalid { get; } = new( null!, null! );
        }

        public IRef<INamedType> AttributeType { get; }

        public ISymbol GetClosestSymbol( CompilationContext compilation ) => this._containingDeclaration.Unwrap().GetClosestSymbol( compilation );

        (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) IRefImpl.GetAttributeData( CompilationContext compilationContext )
            => throw new NotSupportedException();

        string IRefImpl.Name => throw new NotSupportedException();

        IRefStrategy IRefImpl.Strategy => throw new NotSupportedException();

        public IRefImpl Unwrap() => this;

        private ResolvedRef? ResolveAttributeData( AttributeSyntax attributeSyntax, CompilationContext compilation )
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
            var (attributes, symbol) = this._containingDeclaration.Unwrap().GetAttributeData( compilation );

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

        public AttributeRef( AttributeData attributeData, IRef<IDeclaration> containingDeclaration, CompilationContext compilationContext )
        {
            // Note that Roslyn can return an AttributeData that does not belong to the same compilation
            // as the parent symbol, probably because of some bug or optimisation.

            this._target = attributeData;

            this.AttributeType = compilationContext.RefFactory.FromSymbol<INamedType>(
                attributeData.AttributeClass.AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes )
                    .TranslateIfNecessary( compilationContext ) );

            this._containingDeclaration = containingDeclaration.AsRefImpl();
        }

        public AttributeRef(
            IRef<INamedType> attributeType,
            AttributeSyntax attributeSyntax,
            SyntaxNode declaration,
            DeclarationRefTargetKind targetKind,
            CompilationContext compilationContext )
        {
            this.AttributeType = attributeType;
            this._target = attributeSyntax;
            this._containingDeclaration = new SyntaxNodeRef<IDeclaration>( declaration, targetKind, compilationContext );
        }

        public AttributeRef( IRef<INamedType> attributeType, AttributeSyntax attributeSyntax, ISymbol declaration, CompilationContext compilationContext )
        {
            this.AttributeType = attributeType;
            this._target = attributeSyntax;
            this._containingDeclaration = compilationContext.RefFactory.FromSymbol<IDeclaration>( declaration ).AsRefImpl();
        }

        public AttributeRef( AttributeBuilder builder )
        {
            this.AttributeType = builder.Constructor.DeclaringType.ToRef();
            this._target = builder;
            this._containingDeclaration = builder.ContainingDeclaration.ToRef();
        }

        public AttributeRef( AttributeSerializationData serializationData )
        {
            this._target = serializationData;
            this.AttributeType = serializationData.Type;
            this._containingDeclaration = serializationData.ContainingDeclaration.AsRefImpl();
        }

        public SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

        IRefImpl<TOut> IRefImpl<IAttribute>.As<TOut>() => this as IRefImpl<TOut> ?? throw new NotSupportedException();

        public IAttribute GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, genericContext, out var attribute ) )
            {
                throw new AssertionFailedException( "Attempt to resolve an invalid custom attribute." );
            }

            return attribute;
        }

        ICompilationElement? IRef.GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
            => this.GetTargetOrNull( compilation, options, genericContext );

        ICompilationElement IRef.GetTarget( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
            => this.GetTarget( compilation, options, genericContext );

        public IAttribute? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, genericContext, out var attribute ) )
            {
                return null;
            }

            return attribute;
        }

        public IRef<TOut> As<TOut>()
            where TOut : class, ICompilationElement
            => this as IRef<TOut> ?? throw new InvalidCastException();

        private AttributeSyntax? Syntax
            => this._target switch
            {
                null => null,
                AttributeSyntax syntax => syntax,
                AttributeData data => (AttributeSyntax?) data.ApplicationSyntaxReference?.GetSyntax(),
                AttributeBuilder => null,
                _ => throw new AssertionFailedException( $"Unexpected target type '{this._target.GetType()}'." )
            };

        [Obsolete( "This is a hack, and should no longer be used." )]
        public object Target => this._target;

        public bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
        {
            switch ( this._target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.
                    attribute = null;

                    return false;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilation.CompilationContext );

                        if ( resolved == null )
                        {
                            attribute = null;

                            return false;
                        }

                        attribute = new Attribute(
                            resolved.AttributeData,
                            compilation,
                            this._containingDeclaration.GetTarget( compilation, genericContext: genericContext ) );

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
                    attribute = new BuiltAttribute( builder, compilation, genericContext ?? NullGenericContext.Instance );

                    return true;

                case AttributeSerializationData serializationData:
                    attribute = compilation.Factory.GetDeserializedAttribute( serializationData );

                    return true;

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a {this._target.GetType().Name}.'" );
            }
        }

        public bool TryGetAttributeSerializationDataKey( CompilationContext compilationContext, [NotNullWhen( true )] out object? serializationDataKey )
        {
            switch ( this._target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.

                    serializationDataKey = null;

                    return false;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilationContext );

                        if ( resolved == null )
                        {
                            serializationDataKey = null;

                            return false;
                        }

                        serializationDataKey = resolved;

                        return true;
                    }

                case AttributeData:
                case AttributeBuilder:
                case AttributeSerializationData:
                    serializationDataKey = this._target;

                    return true;

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a {this._target.GetType().Name}.'" );
            }
        }

        public bool TryGetAttributeSerializationData(
            CompilationContext compilationContext,
            [NotNullWhen( true )] out AttributeSerializationData? serializationData )
        {
            switch ( this._target )
            {
                case null:
                    // This happens when ResolveAttributeData was already called but was unsuccessful.

                    serializationData = null;

                    return false;

                case AttributeSyntax attributeSyntax:
                    {
                        var resolved = this.ResolveAttributeData( attributeSyntax, compilationContext );

                        if ( resolved == null )
                        {
                            serializationData = null;

                            return false;
                        }

                        serializationData = new AttributeSerializationData( resolved.Parent, resolved.AttributeData, compilationContext );

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
                    throw new AssertionFailedException( $"Don't know how to resolve a {this._target.GetType().Name}.'" );
            }
        }

        ISymbol ISdkRef.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();

        public override string ToString() => $"{this._target} on {this._containingDeclaration}";

        public bool IsSyntax( AttributeSyntax attribute )
            => this._target switch
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

            switch ( this._target )
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
                    if ( builder != other._target )
                    {
                        return false;
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected target type '{this._target.GetType()}'." );
            }

            if ( !RefEqualityComparer<IDeclaration>.Default.Equals( this._containingDeclaration, other._containingDeclaration ) )
            {
                return false;
            }

            return true;
        }

        bool IEquatable<IRef>.Equals( IRef? other ) => other is AttributeRef otherAttributeRef && this.Equals( otherAttributeRef );

        public override int GetHashCode()
        {
            var targetHashCode = this._target switch
            {
                AttributeSyntax syntax => syntax.GetHashCode(),
                AttributeData data => data.ApplicationSyntaxReference?.GetSyntax().GetHashCode() ?? 0,
                AttributeBuilder builder => builder.GetHashCode(),
                _ => throw new AssertionFailedException( $"Unexpected target type '{this._target.GetType()}'." )
            };

            return HashCode.Combine( targetHashCode, RefEqualityComparer<IDeclaration>.Default.GetHashCode( this._containingDeclaration ) );
        }
    }
}