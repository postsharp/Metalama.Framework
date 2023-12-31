﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Utilities.Comparers;

internal static class StructuralSymbolComparerOptionsExtensions
{
    public static bool HasFlagFast( this StructuralSymbolComparerOptions options, StructuralSymbolComparerOptions flag ) => (options & flag) == flag;
}