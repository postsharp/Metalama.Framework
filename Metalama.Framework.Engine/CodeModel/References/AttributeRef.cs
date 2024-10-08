// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// Base implementation of <see cref="IRef"/> for <see cref="IAttribute"/>.
    /// </summary>
    internal abstract class AttributeRef : IRef<IAttribute>, IEquatable<AttributeRef>, IRefImpl
    {
        protected AttributeRef( IFullRef<IDeclaration> containingDeclaration, IRef<INamedType> attributeType )
        {
            this.ContainingDeclaration = containingDeclaration;
            this.AttributeType = attributeType;
        }

        public IFullRef ContainingDeclaration { get; }

        public IRef<INamedType> AttributeType { get; }

        public ISymbol GetClosestContainingSymbol() => this.ContainingDeclaration.GetClosestContainingSymbol();

        public abstract string? Name { get; }

        SerializableDeclarationId IRef.ToSerializableId() => throw new NotSupportedException();

        SerializableDeclarationId IRefImpl.ToSerializableId( CompilationContext compilationContext ) => throw new NotSupportedException();

        IRef<TOut> IRef.As<TOut>() => this as IRef<TOut> ?? throw new NotSupportedException();

        public IAttribute GetTarget( ICompilation compilation, IGenericContext? genericContext = null )
        {
            if ( !this.TryGetTarget( (CompilationModel) compilation, genericContext, out var attribute ) )
            {
                throw new AssertionFailedException( "Attempt to resolve an invalid custom attribute." );
            }

            return attribute;
        }

        ICompilationElement? IRef.GetTargetOrNull( ICompilation compilation, IGenericContext? genericContext )
            => this.GetTargetOrNull( compilation, genericContext );

        public IDurableRef<IAttribute> ToDurable() => throw new NotSupportedException();

        public bool IsDurable => false;

        IRef IRefImpl.ToDurable() => this.ToDurable();

        ICompilationElement IRef.GetTarget( ICompilation compilation, IGenericContext? genericContext ) => this.GetTarget( compilation );

        public IAttribute? GetTargetOrNull( ICompilation compilation, IGenericContext? genericContext = null )
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

        public bool Equals( IRef? other, RefComparison comparison = RefComparison.Default )
        {
            if ( comparison != RefComparison.Default )
            {
                throw new NotSupportedException( "Non-default comparison of attributes is not supported." );
            }

            if ( other is not AttributeRef otherAttributeRef )
            {
                return false;
            }

            return this.Equals( otherAttributeRef );
        }

        public int GetHashCode( RefComparison comparison )
            => comparison switch
            {
                RefComparison.Default => this.GetHashCode(),
                _ => throw new NotSupportedException( "Non-default comparison of attributes is not supported." )
            };

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