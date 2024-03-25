// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// The implementation of <see cref="IMemberRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct MemberRef<T> : IMemberRef<T>, IEquatable<MemberRef<T>>
        where T : class, IMemberOrNamedType
    {
        private readonly Ref<T> _underlying;

        public MemberRef( ISymbol symbol, CompilationContext compilation )
        {
            symbol.AssertValidType<T>();

            this._underlying = new Ref<T>( symbol, compilation );
        }

        public MemberRef( IMemberOrNamedTypeBuilder builder )
        {
            this._underlying = new Ref<T>( builder );
        }

        public MemberRef( in Ref<IDeclaration> declarationRef )
        {
            this._underlying = declarationRef.As<T>();
        }

        public object? Target => this._underlying.Target;

        public SerializableDeclarationId ToSerializableId() => this._underlying.ToSerializableId();

        public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default ) => this._underlying.GetTarget( compilation, options );

        public T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default )
            => this._underlying.GetTargetOrNull( compilation, options );

        public ISymbol GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => this._underlying.GetSymbol( compilation ).AssertNotNull();

        public Ref<T> ToRef() => this._underlying;

        public override string ToString() => this._underlying.ToString();

        public string Name
            => this.Target switch
            {
                ISymbol symbol => symbol.Name,
                IMemberOrNamedTypeBuilder builder => builder.Name,
                _ => throw new AssertionFailedException( $"Unexpected target type '{this.Target?.GetType()}'." )
            };

        public bool IsDefault => this._underlying.IsDefault;

        public ISymbol GetClosestSymbol( CompilationContext compilation ) => this._underlying.GetClosestSymbol( compilation );

        public MemberRef<TCast> As<TCast>()
            where TCast : class, IMemberOrNamedType
            => new( this._underlying.As<IDeclaration>() );

        [Obsolete( "Use comparer.", true )]
        public bool Equals( MemberRef<T> other ) => throw new NotSupportedException( $"Must use {nameof(MemberRefEqualityComparer<T>)}." );

#pragma warning disable CS0809
        [Obsolete( "Use comparer.", true )]
        public override int GetHashCode() => throw new NotSupportedException( $"Must use {nameof(MemberRefEqualityComparer<T>)}." );
#pragma warning restore CS0809
    }
}