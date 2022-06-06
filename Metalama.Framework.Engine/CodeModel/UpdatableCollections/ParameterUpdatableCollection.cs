// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class ParameterUpdatableCollection : UpdatableDeclarationCollection<IParameter>
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
                    action( Ref.FromSymbol<IParameter>( p, this.Compilation.RoslynCompilation ) );
                }

                break;

            case IMethodBaseBuilder builder:
                foreach ( var p in builder.Parameters )
                {
                    action( Ref.FromBuilder<IParameter, IParameterBuilder>( (IParameterBuilder) p ) );
                }

                break;

            default:
                throw new AssertionFailedException();
        }
    }

    public void Add( IParameterBuilder parameterBuilder )
    {
        this.EnsureComplete();
        this.AddItem( parameterBuilder.ToTypedRef<IParameter>() );
    }
}