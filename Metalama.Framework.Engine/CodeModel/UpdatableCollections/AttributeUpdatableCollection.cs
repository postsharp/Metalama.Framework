﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

    // This field is set only when _parent is the ISourceAssemblySymbol.
    private readonly IModuleSymbol? _moduleSymbol;

    public AttributeUpdatableCollection( CompilationModel compilation, Ref<IDeclaration> parent, IModuleSymbol? moduleSymbol ) : base( compilation )
    {
        this._parent = parent;
        this._moduleSymbol = moduleSymbol;
    }

    protected override void PopulateAllItems( Action<AttributeRef> action )
    {
        switch ( this._parent.Target )
        {
            case ISymbol symbol:

                var attributes = this._parent.TargetKind switch
                {
                    DeclarationRefTargetKind.Return => ((IMethodSymbol) symbol).GetReturnTypeAttributes(),
                    DeclarationRefTargetKind.Default => symbol.GetAttributes(),
                    _ => throw new NotImplementedException()
                };

                foreach ( var attribute in attributes )
                {
                    if ( attribute.AttributeConstructor == null )
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

        if ( this._moduleSymbol != null )
        {
            foreach ( var attribute in this._moduleSymbol.GetAttributes() )
            {
                if ( attribute.AttributeConstructor == null )
                {
                    continue;
                }

                action( new AttributeRef( attribute, this._parent ) );
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
            .ToList();

        foreach ( var item in itemsToRemove )
        {
            this.RemoveItem( item );
        }
    }
}