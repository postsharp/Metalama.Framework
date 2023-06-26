// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class AllInterfaceUpdatableCollection : UpdatableDeclarationCollection<INamedType>
{
    private readonly INamedTypeSymbol _declaringType;

    // TODO: This property is written but never read.
    private ImmutableArray<IntroduceInterfaceTransformation> Introductions { get; set; } = ImmutableArray<IntroduceInterfaceTransformation>.Empty;

    public AllInterfaceUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation )
    {
        this._declaringType = declaringType;
    }

    public void Add( IntroduceInterfaceTransformation introduction )
    {
        this.EnsureComplete();
        this.AddItem( introduction.InterfaceType.ToTypedRef() );

        this.Introductions = this.Introductions.Add( introduction );
    }

    protected override void PopulateAllItems( Action<Ref<INamedType>> action )
    {
        foreach ( var i in this._declaringType.AllInterfaces )
        {
            action( new Ref<INamedType>( i, this.Compilation.CompilationContext ) );
        }
    }
}