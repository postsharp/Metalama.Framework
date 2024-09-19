// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class ParameterUpdatableCollection : UpdatableDeclarationCollection<IParameter, IRef<IParameter>>
{
    private readonly IRef<IHasParameters> _parent;

    public ParameterUpdatableCollection( CompilationModel compilation, IRef<IHasParameters> parent ) : base( compilation )
    {
        this._parent = parent;
    }

    protected override void PopulateAllItems( Action<IRef<IParameter>> action )
    {
        switch ( this._parent )
        {
            case ISymbolRef { Symbol: IMethodSymbol method }:
                foreach ( var p in method.Parameters )
                {
                    action( this.RefFactory.FromSymbol<IParameter>( p ) );
                }

                break;

            case ISymbolRef { Symbol: IPropertySymbol { Parameters.IsEmpty: false } indexer }:
                foreach ( var p in indexer.Parameters )
                {
                    action( this.RefFactory.FromSymbol<IParameter>( p ) );
                }

                break;

            case IBuilderRef { Builder: IMethodBaseBuilder builder }:
                foreach ( var p in builder.Parameters )
                {
                    action( this.RefFactory.FromBuilder<IParameter>( p ) );
                }

                break;

            case IBuilderRef { Builder: IIndexerBuilder indexerBuilder }:
                foreach ( var p in indexerBuilder.Parameters )
                {
                    action( this.RefFactory.FromBuilder<IParameter>( p ) );
                }

                break;

            default:
                throw new AssertionFailedException( $"Unexpected parent type: '{this._parent}'." );
        }
    }

    public void Add( IParameterBuilder parameterBuilder )
    {
        this.EnsureComplete();

        var lastParam =
            this.Count > 0
                ? (IRef<IParameter>?) this[this.Count - 1]
                : null;

        if ( lastParam is ISymbolRef { Symbol: IParameterSymbol { IsParams: true } } )
        {
            this.InsertItem( this.Count - 1, parameterBuilder.ToRef() );
        }
        else
        {
            this.AddItem( parameterBuilder.ToRef() );
        }
    }
}