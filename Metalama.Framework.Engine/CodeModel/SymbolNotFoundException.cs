// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

public class SymbolNotFoundException : Exception
{
    public SymbolNotFoundException( string symbolId, Compilation compilation ) : base(
        $"Cannot resolve the symbol '{symbolId}' in '{compilation.AssemblyName}' for '{compilation.GetTargetFramework()}'." ) { }
}