// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal record struct SyntaxTreeVersion( SyntaxTree Tree, bool HasCompileTimeCode, ulong Hash );

internal record class CompilationVersion( Compilation Compilation, ImmutableDictionary<string,SyntaxTreeVersion> SyntaxTrees );