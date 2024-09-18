﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class AllInterfaceUpdatableCollection : UpdatableDeclarationCollection<INamedType>
{
    private readonly IRef<INamedType> _declaringType;

    // TODO: This property is written but never read.
    private ImmutableArray<IntroduceInterfaceTransformation> Introductions { get; set; } = ImmutableArray<IntroduceInterfaceTransformation>.Empty;

    public AllInterfaceUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base( compilation )
    {
        this._declaringType = declaringType;
    }

    public void Add( IntroduceInterfaceTransformation introduction )
    {
        this.EnsureComplete();
        this.AddItem( introduction.InterfaceType.ToRef() );

        this.Introductions = this.Introductions.Add( introduction );
    }

    protected override void PopulateAllItems( Action<IRef<INamedType>> action )
    {
        ((IRefImpl) this._declaringType).Strategy.EnumerateAllImplementedInterfaces( this._declaringType, this.Compilation, action );
    }
}