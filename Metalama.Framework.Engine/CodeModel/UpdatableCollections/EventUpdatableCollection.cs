// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class EventUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IEvent>
{
    public EventUpdatableCollection( CompilationModel compilation, Ref<INamedType> declaringType ) : base( compilation, declaringType.As<INamespaceOrNamedType>() ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol ) => symbol.Kind == SymbolKind.Event && base.IsSymbolIncluded( symbol );

    protected override IEqualityComparer<MemberRef<IEvent>> MemberRefComparer => this.Compilation.CompilationContext.EventRefComparer;
}