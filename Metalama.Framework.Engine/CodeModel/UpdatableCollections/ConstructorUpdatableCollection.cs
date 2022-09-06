// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class ConstructorUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IConstructor>
{
    public ConstructorUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override Func<ISymbol, bool> Predicate
        => m => m.Kind == SymbolKind.Method &&
                ((IMethodSymbol) m).MethodKind is MethodKind.Constructor;

    // TODO: define implicit constructor
}