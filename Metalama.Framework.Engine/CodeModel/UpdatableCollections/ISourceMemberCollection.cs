// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal interface ISourceMemberCollection<T> : ISourceDeclarationCollection<T>
    where T : class, IMemberOrNamedType
{
    ImmutableArray<MemberRef<T>> OfName( string name );
}

internal interface ISourceDeclarationCollection<T> : ISourceDeclarationCollection<T, Ref<T>>
    where T : class, IDeclaration
{
}

internal interface ISourceDeclarationCollection<TDeclaration, TRef> : IReadOnlyList<TRef>
    where TDeclaration : class, IDeclaration
    where TRef : IRefImpl<TDeclaration>, IEquatable<TRef>
{
    CompilationModel Compilation { get; }

    ISourceDeclarationCollection<TDeclaration, TRef> Clone( CompilationModel compilation );
}