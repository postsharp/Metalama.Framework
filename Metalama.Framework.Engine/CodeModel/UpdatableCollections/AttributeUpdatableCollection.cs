// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class AttributeUpdatableCollection : UpdatableDeclarationCollection<IAttribute, AttributeRef>
{
    private readonly IRef<IDeclaration> _parent;

    // HACK! This field is set only when _parent is the ISourceAssemblySymbol.
    private readonly IModuleSymbol? _moduleSymbol;

    public AttributeUpdatableCollection( CompilationModel compilation, IRef<IDeclaration> parent, IModuleSymbol? moduleSymbol ) : base( compilation )
    {
        this._parent = parent;
        this._moduleSymbol = moduleSymbol;

#if DEBUG
        (this._parent.Unwrap() as ISymbolRef)?.Symbol.ThrowIfBelongsToDifferentCompilationThan( compilation.CompilationContext );
#endif
    }

    protected override void PopulateAllItems( Action<IRef<IAttribute>> action )
    {
        this._parent.GetStrategy().EnumerateAttributes( this._parent, this.Compilation, action );

        // HACK!
        if ( this._moduleSymbol != null )
        {
            foreach ( var attribute in this._moduleSymbol.GetAttributes() )
            {
                if ( attribute.AttributeConstructor == null )
                {
                    continue;
                }

                action( new AttributeRef( attribute, this._parent, this.Compilation.CompilationContext ) );
            }
        }
    }

    public void Add( AttributeBuilder attribute )
    {
        this.EnsureComplete();
        this.AddItem( new AttributeRef( attribute ) );
    }

    public void Remove( INamedType namedType )
    {
        this.EnsureComplete();

        var itemsToRemove = this.Where( x => x.GetTarget( namedType.Compilation ).Constructor.DeclaringType.Is( namedType ) )
            .ToReadOnlyList();

        foreach ( var item in itemsToRemove )
        {
            this.RemoveItem( item );
        }
    }
}