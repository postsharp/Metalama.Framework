// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// The implementation of <see cref="IMemberRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct MemberRef<T> : IMemberRef<T>
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

        public object? Target => this._underlying.Target;

        public string? ToSerializableId() => this._underlying.ToSerializableId();

        public T GetTarget( ICompilation compilation ) => this._underlying.GetTarget( compilation );

        public ISymbol? GetSymbol( Compilation compilation ) => this._underlying.GetSymbol( compilation );

        public override string ToString() => this._underlying.ToString();

        public string Name
            => this.Target switch
            {
                ISymbol symbol => symbol.Name,
                IMemberOrNamedTypeBuilder builder => builder.Name,
                _ => throw new AssertionFailedException()
            };
    }
}