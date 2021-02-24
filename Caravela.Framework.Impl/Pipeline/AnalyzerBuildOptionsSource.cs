// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="IBuildOptions"/> that reads the values from <see cref="AnalyzerConfigOptions"/>.
    /// </summary>
    internal class AnalyzerBuildOptionsSource : IBuildOptionsSource
    {
        private readonly AnalyzerConfigOptions _options;

        public AnalyzerBuildOptionsSource( AnalyzerConfigOptionsProvider options )
        {
            this._options = options.GlobalOptions;
        }

        public AnalyzerBuildOptionsSource( AnalyzerConfigOptions options )
        {
            this._options = options;
        }

        public bool TryGetValue( string name, out string? value ) => this._options.TryGetValue( name, out value );
    }
}