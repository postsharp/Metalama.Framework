// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Options
{
    /// <summary>
    /// Exposes project options (typically defined in MSBuild or .editorconfig) in a strongly-typed manner.
    /// The production implementation is <see cref="MSBuildProjectOptions"/> but tests can provide their own implementation.
    /// </summary>
    public interface IProjectOptions : IProjectService
    {
        /// <summary>
        /// Gets the path to a file that gets touched when the project is built.
        /// </summary>
        string? BuildTouchFile { get; }

        /// <summary>
        /// Gets the path to a file that gets touched when the Roslyn analysis process generates new sources.
        /// </summary>
        string? SourceGeneratorTouchFile { get; }

        string? AssemblyName { get; }

        ImmutableArray<object> PlugIns { get; }

        /// <summary>
        /// Gets a value indicating whether the aspect framework is enabled for the current project. If <c>false</c>,
        /// the project will not be modified. 
        /// </summary>
        bool IsFrameworkEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether the output syntax trees must be formatted.
        /// </summary>
        bool FormatOutput { get; }

        bool FormatCompileTimeCode { get; }

        /// <summary>
        /// Gets a value indicating whether the user code processed by Metalama is trusted.
        /// </summary>
        bool IsUserCodeTrusted { get; }

        string? ProjectPath { get; }

        /// <summary>
        /// Gets the short target framework name, for instance <c>net6.0</c>.
        /// </summary>
        string? TargetFramework { get; }

        /// <summary>
        /// Gets the full target framework moniker, for instance <c>.NETCoreApp,Version=v6.0</c>.
        /// </summary>
        string? TargetFrameworkMoniker { get; }

        string? Configuration { get; }

        /// <summary>
        /// Gets a value indicating whether the design time experience is enabled for the project.
        /// </summary>
        bool IsDesignTimeEnabled { get; }

        /// <summary>
        /// Gets a path to a directory that stores additional compilation output files.
        /// </summary>
        string? AdditionalCompilationOutputDirectory { get; }

        /// <summary>
        /// Invoked when project options have been applied globally or contextually through the <see cref="!:ServiceProviderFactory" />,
        /// and are then overridden by options provided by the compiler.
        /// </summary>
        IProjectOptions Apply( IProjectOptions options );

        bool TryGetProperty( string name, out string? value );

        /// <summary>
        /// Gets a value indicating whether the compile-time-only code should be removed from the main compiled assembly.
        /// </summary>
        bool RemoveCompileTimeOnlyCode { get; }

        /// <summary>
        /// Gets a value indicating whether the linker should annotate the code for code coverage.
        /// </summary>
        bool RequiresCodeCoverageAnnotations { get; }

        bool AllowPreviewLanguageFeatures { get; }

        bool RequireOrderedAspects { get; }

        bool IsConcurrentBuildEnabled { get; }

        /// <summary>
        /// Gets the list of packages (given by name only) that should be included in the compile-time project.
        /// </summary>
        ImmutableArray<string> CompileTimePackages { get; }

        /// <summary>
        /// Gets the path to <c>project.assets.json</c>.
        /// </summary>
        string? ProjectAssetsFile { get; }

        /// <summary>
        /// Gets the license set for the project. In production, the value gets populated from <c>MetalamaLicense</c> MSBuild property.
        /// </summary>
        /// <remarks>
        /// This value is used in design-time. In compile-time, the license consumption manager comes from
        /// Metalama.Compiler, which already has the additional license set.
        /// </remarks>
        string? License { get; }

        bool IsTest { get; }
    }
}