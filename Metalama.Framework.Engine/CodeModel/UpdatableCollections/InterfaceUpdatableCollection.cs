// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class InterfaceUpdatableCollection : UpdatableDeclarationCollection<INamedType>
{
    private readonly INamedTypeSymbol _declaringType;

    public ImmutableArray<IntroduceInterfaceTransformation> Introductions { get; private set; } = ImmutableArray<IntroduceInterfaceTransformation>.Empty;

    public InterfaceUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation )
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
        foreach ( var i in this._declaringType.Interfaces )
        {
            action( new Ref<INamedType>( i, this.Compilation.CompilationContext ) );
        }
    }
}