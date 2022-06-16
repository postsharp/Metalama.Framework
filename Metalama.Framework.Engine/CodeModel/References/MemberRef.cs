// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
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

        public MemberRef( ISymbol symbol, Compilation compilation )
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

        public MemberRef( in Ref<T> declarationRef )
        {
            this._underlying = declarationRef;
        }

        public object? Target => this._underlying.Target;

        public DeclarationSerializableId ToSerializableId() => this._underlying.ToSerializableId();

        public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default ) => this._underlying.GetTarget( compilation, options );

        public ISymbol? GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => this._underlying.GetSymbol( compilation );

        public Ref<T> ToRef() => this._underlying;

        public override string ToString() => this._underlying.ToString();

        public string Name
            => this.Target switch
            {
                ISymbol symbol => symbol.Name,
                IMemberOrNamedTypeBuilder builder => builder.Name,
                _ => throw new AssertionFailedException()
            };

        public bool IsDefault => this._underlying.IsDefault;

        public MemberRef<TCast> As<TCast>()
            where TCast : class, IMemberOrNamedType
            => new( this._underlying.As<IDeclaration>() );

        public bool Equals( MemberRef<T> other ) => MemberRefEqualityComparer<T>.Default.Equals( this, other );

        public override int GetHashCode() => MemberRefEqualityComparer<T>.Default.GetHashCode( this );
    }
}