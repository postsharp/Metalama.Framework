// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.Introspection.References;

internal static class ChildKindHelper
{
    public static ChildKinds ToChildKinds( ReferenceGraphChildKinds kind )
        => kind switch
        {
            ReferenceGraphChildKinds.All => ChildKinds.All,
            ReferenceGraphChildKinds.None => ChildKinds.None,
            ReferenceGraphChildKinds.ContainingDeclaration => ChildKinds.ContainingDeclaration,
            ReferenceGraphChildKinds.DerivedType => ChildKinds.DerivedType,
            _ => throw new ArgumentOutOfRangeException()
        };
}