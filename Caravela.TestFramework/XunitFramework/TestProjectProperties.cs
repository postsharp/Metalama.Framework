// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Caravela.TestFramework.XunitFramework
{
    public record TestProjectProperties( string ProjectDirectory, ImmutableArray<string> PreprocessorSymbols, string TargetFramework );
}