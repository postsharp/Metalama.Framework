// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

public sealed class SymbolNotFoundException : Exception
{
    public SymbolNotFoundException( string symbolId, Compilation compilation ) : base(
        $"Cannot resolve the symbol '{symbolId}' in '{compilation.AssemblyName}' for '{compilation.GetTargetFramework()}'." ) { }
}