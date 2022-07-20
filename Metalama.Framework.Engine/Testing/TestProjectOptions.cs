// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Metalama.Framework.Engine.Testing
{
    /// <summary>
    /// An implementation of <see cref="IProjectOptions"/> and <see cref="IPathOptions"/> that can be used in tests.
    /// </summary>
    public class TestProjectOptions : DefaultProjectOptions, IDisposable
    {
        private readonly ImmutableDictionary<string, string> _properties;
        private readonly Lazy<string> _baseDirectory;
        private readonly Lazy<string> _projectDirectory;

        public TestProjectOptions(
            ImmutableDictionary<string, string>? properties = null,
            ImmutableArray<object> plugIns = default,
            bool formatOutput = false,
            bool formatCompileTimeCode = false )
        {
            this.PlugIns = plugIns.IsDefault ? ImmutableArray<object>.Empty : plugIns;

            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;
            var baseDirectory = Path.Combine( Path.GetTempPath(), "Metalama", "Tests", RandomIdGenerator.GenerateId() );
            this._baseDirectory = CreateDirectoryLazy( baseDirectory );

            var compileTimeProjectCacheDirectory = Path.Combine( this.BaseDirectory, "Cache" );
            var compileTimeProjectCacheDirectoryLazy = CreateDirectoryLazy( compileTimeProjectCacheDirectory );

            var settingsDirectory = Path.Combine( baseDirectory, "Settings" );
            var settingsDirectoryLazy = CreateDirectoryLazy( settingsDirectory );

            this.PathOptions = new TestPathOptions( settingsDirectoryLazy, compileTimeProjectCacheDirectoryLazy );

            var projectDirectory = Path.Combine( baseDirectory, "Project" );
            this._projectDirectory = CreateDirectoryLazy( projectDirectory );

            this.FormatOutput = formatOutput;
            this.FormatCompileTimeCode = formatCompileTimeCode;
        }

        public override string ProjectId { get; } = Guid.NewGuid().ToString();

        public TestPathOptions PathOptions { get; }

        private static Lazy<string> CreateDirectoryLazy( string path )
            => new(
                () =>
                {
                    Directory.CreateDirectory( path );

                    return path;
                } );

        public override ImmutableArray<object> PlugIns { get; }

        public string BaseDirectory => this._baseDirectory.Value;

        public override bool FormatOutput { get; }

        public override bool FormatCompileTimeCode { get; }

        public string ProjectDirectory => this._projectDirectory.Value;

        public override bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._properties.TryGetValue( name, out value );

        public void Dispose()
        {
            if ( Directory.Exists( this.BaseDirectory ) )
            {
                TestExecutionContext.RegisterDisposeAction( () => Directory.Delete( this.BaseDirectory, true ) );
            }
        }
    }
}