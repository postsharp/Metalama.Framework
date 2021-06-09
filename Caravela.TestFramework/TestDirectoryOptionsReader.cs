// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    internal class TestDirectoryOptionsReader
    {
        private static readonly ConditionalWeakTable<Assembly, TestDirectoryOptionsReader> _instances = new ConditionalWeakTable<Assembly, TestDirectoryOptionsReader>();

        public string ProjectDirectory { get; }

        private readonly ConcurrentDictionary<string, TestDirectoryOptions> _cache = new( StringComparer.OrdinalIgnoreCase );

        public TestDirectoryOptionsReader( string projectDirectory )
        {
            this.ProjectDirectory = projectDirectory;
        }

        public static TestDirectoryOptionsReader GetInstance( Assembly assembly )
        {
            if ( _instances.TryGetValue( assembly, out var instance ) )
            {
                return instance;
            }
            else
            {
                var assemblyInfo = new ReflectionAssemblyInfo( assembly );
                var discoverer = new TestDiscoverer( assemblyInfo );
                instance = new TestDirectoryOptionsReader( discoverer.FindProjectDirectory() );
                _instances.AddOrUpdate( assembly, instance );
            }

            return instance;
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