// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class OperatorUpdatableCollection : NonUniquelyNamedMemberUpdatableCollection<IMethod>
{
    public OperatorUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override Func<ISymbol, bool> Predicate
        => m => m switch
        {
            IMethodSymbol method =>
                method switch
                {
                    { MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion } => true,
                    _ => false
                },
            _ => false
        };
}