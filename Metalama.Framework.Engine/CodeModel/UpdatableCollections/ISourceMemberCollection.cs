// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal interface ISourceMemberCollection<T> : ISourceDeclarationCollection<T>
    where T : class, INamedDeclaration
{
    ImmutableArray<IRef<T>> OfName( string name );
}

internal interface ISourceDeclarationCollection<T> : ISourceDeclarationCollection<T, IRef<T>>
    where T : class, IDeclaration;

internal interface ISourceDeclarationCollection<TDeclaration, out TRef> : IReadOnlyList<TRef>
    where TDeclaration : class, IDeclaration
    where TRef : IRef<TDeclaration>
{
    CompilationModel Compilation { get; }

    ISourceDeclarationCollection<TDeclaration, TRef> Clone( CompilationModel compilation );
}