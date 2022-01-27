// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Metalama.Framework.Engine.Testing
{
    /// <summary>
    /// An implementation of <see cref="IProjectOptions"/> and <see cref="IPathOptions"/> that can be used in tests.
    /// </summary>
    public class TestProjectOptions : DefaultPathOptions, IProjectOptions, IDisposable
    {
        private readonly ImmutableDictionary<string, string> _properties;
        private readonly Lazy<string> _baseDirectory;
        private readonly Lazy<string> _settingsDirectory;
        private readonly Lazy<string> _projectDirectory;
        private readonly Lazy<string> _compileTimeProjectCacheDirectory;

        public TestProjectOptions( ImmutableDictionary<string, string>? properties = null )
        {
            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;
            var baseDirectory = Path.Combine( Path.GetTempPath(), "Metalama", "Tests", Guid.NewGuid().ToString() );
            this._baseDirectory = CreateDirectoryLazy( baseDirectory );

            var compileTimeProjectCacheDirectory = Path.Combine( this.BaseDirectory, "Cache" );
            this._compileTimeProjectCacheDirectory = CreateDirectoryLazy( compileTimeProjectCacheDirectory );

            var settingsDirectory = Path.Combine( baseDirectory, "Settings" );
            this._settingsDirectory = CreateDirectoryLazy( settingsDirectory );

            var projectDirectory = Path.Combine( baseDirectory, "Project" );
            this._projectDirectory = CreateDirectoryLazy( projectDirectory );
        }

        private static Lazy<string> CreateDirectoryLazy( string path )
            => new(
                () =>
                {
                    Directory.CreateDirectory( path );

                    return path;
                } );

        // Don't create crash reports for user exceptions so we have deterministic error messages.
        public override string? GetNewCrashReportPath() => null;

        public string BaseDirectory => this._baseDirectory.Value;

        public override string CompileTimeProjectCacheDirectory => this._compileTimeProjectCacheDirectory.Value;

        public override string SettingsDirectory => this._settingsDirectory.Value;

        public string ProjectId => throw new NotSupportedException();

        public virtual string? BuildTouchFile => null;

        public string? SourceGeneratorTouchFile => null;

        public string? AssemblyName => null;

        public ImmutableArray<object> PlugIns => ImmutableArray<object>.Empty;

        public bool IsFrameworkEnabled => true;

        public bool FormatOutput => false;

        public bool FormatCompileTimeCode { get; set; }

        public bool IsUserCodeTrusted => true;

        public string? ProjectPath => null;

        public string? TargetFramework => "net5.0";

        public string? Configuration => "Debug";

        public string ProjectDirectory => this._projectDirectory.Value;

        public IProjectOptions Apply( IProjectOptions options ) => options;

        public bool IsDesignTimeEnabled => true;

        public string? AdditionalCompilationOutputDirectory => null;

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._properties.TryGetValue( name, out value );

        public void Dispose()
        {
            if ( Directory.Exists( this.BaseDirectory ) )
            {
                TestExecutionContext.RegisterDisposeAction( () => Directory.Delete( this.BaseDirectory, true ) );
            }
        }
    }
}