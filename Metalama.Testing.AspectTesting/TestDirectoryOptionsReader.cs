// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;
using System.IO;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Reads and caches the <c>metalamaTests.json</c> files.
    /// </summary>
    internal sealed class TestDirectoryOptionsReader
    {
        public string ProjectDirectory { get; }

        private readonly ConcurrentDictionary<string, TestDirectoryOptions> _cache = new( StringComparer.OrdinalIgnoreCase );

        public TestDirectoryOptionsReader( string projectDirectory )
        {
            this.ProjectDirectory = projectDirectory;
        }

        public TestDirectoryOptions GetDirectoryOptions( string directory ) => this._cache.GetOrAdd( directory, this.GetDirectoryOptionsImpl );

        private TestDirectoryOptions GetDirectoryOptionsImpl( string directory )
        {
            // Read the json file in the directory.
            var optionsPath = Path.Combine( directory, "metalamaTests.json" );
            var options = File.Exists( optionsPath ) ? TestDirectoryOptions.ReadFile( optionsPath ) : new TestDirectoryOptions();

            // Apply settings from the parent directory.
            if ( !directory.Equals( this.ProjectDirectory, StringComparison.OrdinalIgnoreCase ) )
            {
                var baseOptions = this.GetDirectoryOptions( Path.GetDirectoryName( directory )! );
                options.ApplyBaseOptions( baseOptions );
            }

            return options;
        }
    }
}