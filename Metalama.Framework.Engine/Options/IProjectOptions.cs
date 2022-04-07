// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Options
{
    /// <summary>
    /// Exposes project options (typically defined in MSBuild or .editorconfig) in a strongly-typed manner.
    /// The production implementation is <see cref="ProjectOptions"/> but tests can provide their own implementation.
    /// </summary>
    public interface IProjectOptions : IService
    {
        string ProjectId { get; }

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

        string? TargetFramework { get; }

        string? Configuration { get; }

        /// <summary>
        /// Invoked when project options have been applied globally or contextually through the <see cref="ServiceProviderFactory"/>,
        /// and are then overridden by options provided by the compiler.
        /// </summary>
        IProjectOptions Apply( IProjectOptions options );

        bool TryGetProperty( string name, [NotNullWhen( true )] out string? value );

        /// <summary>
        /// Gets a value indicating whether the design time experience is enabled for the project.
        /// </summary>
        bool IsDesignTimeEnabled { get; }

        /// <summary>
        /// Gets a path to a directory that stores additional compilation output files.
        /// </summary>
        string? AdditionalCompilationOutputDirectory { get; }

        string? DotNetSdkDirectory { get; }
    }
}