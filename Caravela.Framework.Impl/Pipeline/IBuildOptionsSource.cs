// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Gives access to configuration options (typically values pulled from MSBuild). The
    /// typical implementation is <see cref="AnalyzerBuildOptionsSource"/>, but other implementations can be used for testing.
    /// </summary>
    public interface IBuildOptionsSource
    {
        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue( string name, out string? value );
    }
}