// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal interface IUpdatableCollection<T> : IReadOnlyList<IRef<T>>
    where T : class, IDeclaration
{
    CompilationModel Compilation { get; }

    IUpdatableCollection<T> Clone( CompilationModel compilation );

    ImmutableArray<IRef<T>> OfName( string name );
}