// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class EventUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IEvent>
{
    public EventUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol ) => symbol.Kind == SymbolKind.Event && base.IsSymbolIncluded( symbol );
}