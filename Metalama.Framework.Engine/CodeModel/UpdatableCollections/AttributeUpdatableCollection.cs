// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class AttributeUpdatableCollection : DeclarationUpdatableCollection<IAttribute>
{
    private readonly IRef<IDeclaration> _parent;

    public AttributeUpdatableCollection( CompilationModel compilation, IRef<IDeclaration> parent ) : base( compilation )
    {
        this._parent = parent;

#if DEBUG
        (this._parent as ISymbolRef)?.Symbol.ThrowIfBelongsToDifferentCompilationThan( compilation.CompilationContext );
#endif
    }

    protected override void PopulateAllItems( Action<IRef<IAttribute>> action )
    {
        this._parent.GetCollectionStrategy().EnumerateAttributes( this._parent, this.Compilation, action );
    }

    public void Add( AttributeBuilder attribute )
    {
        this.EnsureComplete();
        this.AddItem( new BuilderAttributeRef( attribute ) );
    }

    public void Remove( INamedType namedType )
    {
        this.EnsureComplete();

        // TODO: Do not resolve the AttributeRef.
        var itemsToRemove = this.Where( x => x.GetTarget( namedType.Compilation ).Constructor.DeclaringType.Is( namedType ) )
            .ToReadOnlyList();

        foreach ( var item in itemsToRemove )
        {
            this.RemoveItem( item );
        }
    }
}