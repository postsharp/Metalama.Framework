// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Utilities;

internal static class SourceGeneratorHelper
{
    internal static bool IsGeneratedFile( SyntaxTree syntaxTree )
        => syntaxTree.FilePath.StartsWith( "Metalama.Framework.CompilerExtensions", StringComparison.Ordinal );
}