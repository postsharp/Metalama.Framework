// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.IO;

namespace Caravela.TestFramework
{
    internal class TestDirectoryOptionsReader
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
            var optionsPath = Path.Combine( directory, "caravelaTests.json" );
            var options = File.Exists( optionsPath ) ? TestDirectoryOptions.ReadFile( optionsPath ) : new TestDirectoryOptions();

            // Apply settings from the parent directory.
            if ( !directory.Equals( this.ProjectDirectory, StringComparison.OrdinalIgnoreCase ) )
            {
                var baseOptions = this.GetDirectoryOptions( Path.GetDirectoryName( directory )! );
                options.ApplyDirectoryOptions( baseOptions );
            }

            return options;
        }
    }
}