// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class MethodUpdatableCollection : NonUniquelyNamedMemberUpdatableCollection<IMethod>
{
    public MethodUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override Func<ISymbol, bool> Predicate
        => m => m switch
        {
            IMethodSymbol method =>
                method switch
                {
                    { Name: "<Main>$", ContainingType: { Name: "Program" } } => false,
                    { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } => false,
                    { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } => false,
                    { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise } => false,
                    { MethodKind: MethodKind.Destructor } => false,
                    { MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion } => false,
                    _ => true
                },
            _ => false
        };
}