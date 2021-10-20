// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Caravela.TestFramework
{
    // ReSharper disable once CommentTypo

    /// <summary>
    /// Properties of a test project.
    /// </summary>
    /// <param name="ProjectDirectory">Root directory of the project.</param>
    /// <param name="PreprocessorSymbols">List of preprocessor symbols.</param>
    /// <param name="TargetFramework">Identifier of the target framework, as set in MSBuild (e.g. <c>net5.0</c>, <c>netframework4.8</c>, ...</param>
    public record TestProjectProperties( string ProjectDirectory, ImmutableArray<string> PreprocessorSymbols, string TargetFramework );
}