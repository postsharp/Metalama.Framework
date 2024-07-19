// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

#if ROSLYN_4_4_0_OR_GREATER
using System.Linq;
#endif

// ReSharper disable ClassCanBeSealed.Global, InconsistentNaming

namespace Metalama.Framework.Engine.Options
{
    public partial class MSBuildProjectOptions
    {
        /// <summary>
        /// An implementation of <see cref="IProjectOptions"/> that reads the values from <see cref="AnalyzerConfigOptions"/>.
        /// </summary>
        private sealed class OptionsAdapter : IProjectOptionsSource
        {
            private const string _prefix = "build_property.";

            private readonly AnalyzerConfigOptions _options;

            public OptionsAdapter( AnalyzerConfigOptions options )
            {
                this._options = options;
            }

            public bool TryGetValue( string name, out string? value ) => this._options.TryGetValue( $"{_prefix}{name}", out value );

            public IEnumerable<string> PropertyNames
#if ROSLYN_4_4_0_OR_GREATER
                => this._options.Keys.Where( key => key.StartsWith( _prefix, StringComparison.Ordinal ) ).Select( key => key.Substring( _prefix.Length ) );
#else
                => MSBuildPropertyNames.All;
#endif
        }

        public override IProjectOptions Apply( IProjectOptions options ) => throw new NotSupportedException();
    }
}