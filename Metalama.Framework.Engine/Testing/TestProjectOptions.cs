// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Metalama.Framework.Engine.Testing
{
    /// <summary>
    /// An implementation of <see cref="IProjectOptions"/> that can be used in tests.
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
            bool formatCompileTimeCode = false,
            ImmutableArray<Assembly> additionalAssemblies = default,
            bool requireOrderedAspects = false )
        {
            this.PlugIns = plugIns.IsDefault ? ImmutableArray<object>.Empty : plugIns;

            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;

            // We don't use the backstage TempFileManager because it would generate paths that are too long.
            var baseDirectory = Path.Combine( Path.GetTempPath(), "Metalama", "Tests", Guid.NewGuid().ToString() );
            this._baseDirectory = CreateDirectoryLazy( baseDirectory );

            var projectDirectory = Path.Combine( baseDirectory, "Project" );
            this._projectDirectory = CreateDirectoryLazy( projectDirectory );

            this.FormatOutput = formatOutput;
            this.FormatCompileTimeCode = formatCompileTimeCode;
            this.AdditionalAssemblies = additionalAssemblies;
            this.RequireOrderedAspects = requireOrderedAspects;
        }

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

        public override bool RequireOrderedAspects { get; }

        public ImmutableArray<Assembly> AdditionalAssemblies { get; }

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