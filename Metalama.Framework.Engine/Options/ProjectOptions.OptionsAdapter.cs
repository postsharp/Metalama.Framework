// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Options
{
    // ReSharper disable once InconsistentNaming
    public partial class MSBuildProjectOptions
    {
        /// <summary>
        /// An implementation of <see cref="IProjectOptions"/> that reads the values from <see cref="AnalyzerConfigOptions"/>.
        /// </summary>
        private sealed class OptionsAdapter : IProjectOptionsSource
        {
            private readonly AnalyzerConfigOptions _options;

            public OptionsAdapter( AnalyzerConfigOptions options )
            {
                this._options = options;
            }

            public bool TryGetValue( string name, out string? value ) => this._options.TryGetValue( $"build_property.{name}", out value );
        }

        public override IProjectOptions Apply( IProjectOptions options ) => throw new NotSupportedException();
    }
}