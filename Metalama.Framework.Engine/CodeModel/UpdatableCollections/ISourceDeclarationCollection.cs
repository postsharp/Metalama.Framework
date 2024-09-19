// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal interface ISourceDeclarationCollection<TDeclaration, TRef> : IReadOnlyList<TRef>
    where TDeclaration : class, IDeclaration
    where TRef : IRef
{
    CompilationModel Compilation { get; }

    ISourceDeclarationCollection<TDeclaration, TRef> Clone( CompilationModel compilation );

    ImmutableArray<TRef> OfName( string name );
}