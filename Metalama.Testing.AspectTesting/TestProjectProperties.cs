// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.AspectTesting.Licensing;
using System;
using System.Collections.Immutable;

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// Properties of a test project.
/// </summary>
/// <param name="ProjectDirectory">Root directory of the project, or <c>null</c> when executed from Aspect Workbench.</param>
/// <param name="PreprocessorSymbols">List of preprocessor symbols.</param>
/// <param name="TargetFramework">Identifier of the target framework, as set in MSBuild (e.g. <c>net6.0</c>, <c>netframework4.8</c>, ...</param>
internal sealed class TestProjectProperties
{
    private readonly string? _projectDirectory;

    public string ProjectDirectory => this._projectDirectory ?? throw new InvalidOperationException();

    public ImmutableArray<string> PreprocessorSymbols { get; }

    public string TargetFramework { get; }

    public ImmutableArray<string> IgnoredWarnings { get; }

    internal TestFrameworkLicenseStatus? License { get; }

    internal TestProjectProperties(
        string? projectDirectory,
        ImmutableArray<string> preprocessorSymbols,
        string targetFramework,
        ImmutableArray<string> ignoredWarnings,
        TestFrameworkLicenseStatus? license = null )
    {
        this._projectDirectory = projectDirectory;
        this.PreprocessorSymbols = preprocessorSymbols;
        this.TargetFramework = targetFramework;
        this.IgnoredWarnings = ignoredWarnings;
        this.License = license;
    }
}