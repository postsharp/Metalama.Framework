﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class ParameterUpdatableCollection : UpdatableDeclarationCollection<IParameter>
{
    private readonly Ref<IHasParameters> _parent;

    public ParameterUpdatableCollection( CompilationModel compilation, Ref<IHasParameters> parent ) : base( compilation )
    {
        this._parent = parent;
    }

    protected override void PopulateAllItems( Action<Ref<IParameter>> action )
    {
        switch ( this._parent.Target )
        {
            case IMethodSymbol method:
                foreach ( var p in method.Parameters )
                {
                    action( Ref.FromSymbol<IParameter>( p, this.Compilation.CompilationContext ) );
                }

                break;

            case IMethodBaseBuilder builder:
                foreach ( var p in builder.Parameters )
                {
                    action( Ref.FromBuilder<IParameter, IParameterBuilder>( p ) );
                }

                break;

            case IPropertySymbol { Parameters.IsEmpty: false } indexer:
                foreach ( var p in indexer.Parameters )
                {
                    action( Ref.FromSymbol<IParameter>( p, this.Compilation.CompilationContext ) );
                }

                break;

            case IIndexerBuilder indexerBuilder:
                foreach ( var p in indexerBuilder.Parameters )
                {
                    action( Ref.FromBuilder<IParameter, IParameterBuilder>( p ) );
                }

                break;

            default:
                throw new AssertionFailedException( $"Unexpected parent type: '{this._parent.Target?.GetType()}'." );
        }
    }

    public void Add( IParameterBuilder parameterBuilder )
    {
        this.EnsureComplete();
        this.AddItem( parameterBuilder.ToTypedRef<IParameter>() );
    }
}