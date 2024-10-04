// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class EventUpdatableCollection : UniquelyNamedUpdatableCollection<IEvent>
{
    public EventUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base(
        compilation,
        declaringType.As<INamespaceOrNamedType>() ) { }

    protected override DeclarationKind ItemsDeclarationKind => DeclarationKind.Event;
}