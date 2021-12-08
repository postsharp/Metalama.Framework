// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Options
{
    public partial class ProjectOptions
    {
        /// <summary>
        /// An implementation of <see cref="IProjectOptions"/> that reads the values from <see cref="AnalyzerConfigOptions"/>.
        /// </summary>
        private class OptionsAdapter : IProjectOptionsSource
        {
            private readonly AnalyzerConfigOptions _options;

            public OptionsAdapter( AnalyzerConfigOptionsProvider options )
            {
                this._options = options.GlobalOptions;
            }

            public OptionsAdapter( AnalyzerConfigOptions options )
            {
                this._options = options;
            }

            public bool TryGetValue( string name, out string? value ) => this._options.TryGetValue( name, out value );
        }

        public IProjectOptions Apply( IProjectOptions options ) => throw new NotSupportedException();
    }
}