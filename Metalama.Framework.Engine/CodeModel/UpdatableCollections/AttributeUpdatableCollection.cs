// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
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

    protected override void PopulateAllItems( Action<IFullRef<IAttribute>> action )
    {
        this._parent.AsFullRef().EnumerateAttributes( this.Compilation, action );
    }

    public void Add( AttributeBuilderData attribute )
    {
        this.EnsureComplete();
        this.AddItem( attribute.ToRef() );
    }

    public void Remove( IRef<INamedType> namedType )
    {
        this.EnsureComplete();

        var itemsToRemove = this.Where( x => ((AttributeRef) x).AttributeType.IsConvertibleTo( namedType ) );

        foreach ( var item in itemsToRemove )
        {
            this.RemoveItem( item );
        }
    }
}