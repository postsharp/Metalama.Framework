// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Options
{
    /// <summary>
    /// Exposes project options (typically defined in MSBuild or .editorconfig) in a strongly-typed manner.
    /// The production implementation is <see cref="ProjectOptions"/> but tests can provide their own implementation.
    /// </summary>
    public interface IProjectOptions : IDebuggingOptions
    {
        string ProjectId { get; }

        string? BuildTouchFile { get; }

        string? AssemblyName { get; }

        ImmutableArray<object> PlugIns { get; }

        /// <summary>
        /// Gets a value indicating whether the aspect framework is enabled for the current project. If <c>false</c>,
        /// the project will not be modified. 
        /// </summary>
        bool IsFrameworkEnabled { get; }
    }
}