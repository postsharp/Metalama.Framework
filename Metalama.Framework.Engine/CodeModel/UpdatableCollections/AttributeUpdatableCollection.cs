// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class AttributeUpdatableCollection : UpdatableDeclarationCollection<IAttribute, AttributeRef>
{
    private readonly Ref<IDeclaration> _parent;

    public AttributeUpdatableCollection( CompilationModel compilation, Ref<IDeclaration> parent ) : base( compilation )
    {
        this._parent = parent;
    }

    protected override void PopulateAllItems( Action<AttributeRef> action )
    {
        switch ( this._parent.Target )
        {
            case ISymbol symbol:
                foreach ( var attribute in symbol.GetAttributes() )
                {
                    if ( attribute.AttributeClass == null || attribute.AttributeClass is IErrorTypeSymbol )
                    {
                        continue;
                    }
                    
                    action( new AttributeRef( attribute, this._parent ) );
                }

                break;

            case IDeclarationBuilder builder:
                foreach ( var attribute in builder.Attributes )
                {
                    action( new AttributeRef( (AttributeBuilder) attribute ) );
                }

                break;

            default:
                throw new AssertionFailedException();
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

        var itemsToRemove = this.Where(
                a => a.AttributeTypeName == namedType.Name && a.TryGetTarget( namedType.GetCompilationModel(), out var attribute )
                                                           && attribute.Type.Is( namedType ) )
            .ToList();

        foreach ( var item in itemsToRemove )
        {
            this.RemoveItem( item );
        }
    }
}