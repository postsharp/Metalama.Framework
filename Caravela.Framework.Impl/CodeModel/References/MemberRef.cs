// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// The implementation of <see cref="IMemberRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct MemberRef<T> : IMemberRef<T>
        where T : class, IMemberOrNamedType
    {
        public MemberRef( ISymbol symbol )
        {
            symbol.AssertValidType<T>();

            this.Target = symbol;
        }

        public MemberRef( IMemberOrNamedTypeBuilder builder )
        {
            this.Target = builder;
        }

        public MemberRef( in DeclarationRef<IDeclaration> declarationRef )
        {
            this.Target = declarationRef.Target;
        }

        public object? Target { get; }

        public T Resolve( CompilationModel compilation ) => DeclarationRef<T>.Resolve( this.Target, compilation );

        public ISymbol GetSymbol( Compilation compilation ) => this.Target as ISymbol ?? throw new InvalidOperationException();

        public string Name
            => this.Target switch
            {
                ISymbol symbol => symbol.Name,
                IMemberOrNamedTypeBuilder builder => builder.Name,
                _ => throw new AssertionFailedException()
            };

        public override string ToString() => this.Target?.ToString() ?? "null";
    }
}