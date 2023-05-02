﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class MethodUpdatableCollection : NonUniquelyNamedMemberUpdatableCollection<IMethod>
{
    public MethodUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
        => base.IsSymbolIncluded( symbol ) && symbol switch
        {
            IMethodSymbol method =>
                method switch
                {
                    { MethodKind: MethodKind.Ordinary, CanBeReferencedByName: false } => false, // Program.<Main>$ and SomeRecord.<Clone>$
                    { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } => false,
                    { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } => false,
                    { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise } => false,
                    { MethodKind: MethodKind.Destructor } => false,
                    _ => true
                },
            _ => false
        };

    protected override IEqualityComparer<MemberRef<IMethod>> MemberRefComparer => this.Compilation.CompilationContext.MethodRefComparer;
}