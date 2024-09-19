// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal abstract class AttributeRef : IRefImpl<IAttribute>, IEquatable<AttributeRef>
    {
        protected AttributeRef( IRef<IDeclaration> containingDeclaration, IRef<INamedType> attributeType, CompilationContext compilationContext )
        {
            this.ContainingDeclaration = containingDeclaration;
            this.AttributeType = attributeType;
            this.CompilationContext = compilationContext;
        }

        public IRef<IDeclaration> ContainingDeclaration { get; }

        public IRef<INamedType> AttributeType { get; }

        public CompilationContext CompilationContext { get; }

        public ISymbol GetClosestSymbol() => this.ContainingDeclaration.AsRefImpl().GetClosestSymbol();

        (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) IRefImpl.GetAttributeData() => throw new NotSupportedException();

        string IRefImpl.Name => throw new NotSupportedException();

        IRefStrategy IRefImpl.Strategy => throw new NotSupportedException();

        SerializableDeclarationId IRef.ToSerializableId() => throw new NotSupportedException();

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

        protected abstract AttributeSyntax? AttributeSyntax { get; }

        public abstract bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute );

        public abstract bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey );

        public abstract bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData );

        ISymbol ISdkRef.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();

        public bool IsSyntax( AttributeSyntax attribute ) => this.AttributeSyntax == attribute;

        public virtual bool Equals( AttributeRef? other )
        {
            if ( other == null )
            {
                return false;
            }

            var thisSyntax = this.AttributeSyntax;
            var otherSyntax = other.AttributeSyntax;

            if ( thisSyntax == null )
            {
                throw new AssertionFailedException( "Expected that the method would be overridden." );
            }

            return thisSyntax.Equals( otherSyntax );
        }

        protected abstract int GetHashCodeCore();

        bool IEquatable<IRef>.Equals( IRef? other ) => other is AttributeRef otherAttributeRef && this.Equals( otherAttributeRef );

        public override int GetHashCode() => this.GetHashCodeCore();
    }
}