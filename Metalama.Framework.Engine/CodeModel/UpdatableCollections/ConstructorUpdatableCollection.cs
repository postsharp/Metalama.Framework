﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class ConstructorUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IConstructor>
{
    public ConstructorUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
        => symbol.Kind == SymbolKind.Method &&
           ((IMethodSymbol) symbol).MethodKind is MethodKind.Constructor && base.IsSymbolIncluded( symbol );

    // TODO: define implicit constructor
    protected override IEqualityComparer<MemberRef<IConstructor>> MemberRefComparer => this.Compilation.CompilationContext.ConstructorRefComparer;
}