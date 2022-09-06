// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Utilities;

public static class SourceGeneratorHelper
{
    public static bool IsGeneratedFile( SyntaxTree syntaxTree )
        => syntaxTree.FilePath.StartsWith( "Metalama.Framework.CompilerExtensions", StringComparison.Ordinal );
}