// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Caravela.Framework.Impl.Testing
{
    /// <summary>
    /// An implementation of <see cref="IProjectOptions"/> and <see cref="IPathOptions"/> that can be used in tests.
    /// </summary>
    public class TestProjectOptions : DefaultPathOptions, IProjectOptions, IDisposable
    {
        private readonly ImmutableDictionary<string, string> _properties;

        public TestProjectOptions( ImmutableDictionary<string, string>? properties = null )
        {
            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;
            this.BaseDirectory = Path.Combine( Path.GetTempPath(), "Caravela", "Tests", Guid.NewGuid().ToString() );

            var compileTimeProjectCacheDirectory = Path.Combine( this.BaseDirectory, "Cache" );
            this.CompileTimeProjectCacheDirectory = compileTimeProjectCacheDirectory;
            Directory.CreateDirectory( compileTimeProjectCacheDirectory );

            var settingsDirectory = Path.Combine( this.BaseDirectory, "Settings" );
            this.SettingsDirectory = settingsDirectory;
            Directory.CreateDirectory( settingsDirectory );

            var projectDirectory = Path.Combine( this.BaseDirectory, "Project" );
            this.ProjectDirectory = projectDirectory;
            Directory.CreateDirectory( projectDirectory );
        }

        // Don't create crash reports for user exceptions so we have deterministic error messages.
        public override string? GetNewCrashReportPath() => null;

        public string BaseDirectory { get; }

        public bool DebugCompilerProcess => false;

        public bool DebugAnalyzerProcess => false;

        public bool DebugIdeProcess => false;

        public override string CompileTimeProjectCacheDirectory { get; }

        public override string SettingsDirectory { get; }

        public string ProjectId => throw new NotSupportedException();

        public virtual string? BuildTouchFile => null;

        public string? AssemblyName => null;

        public ImmutableArray<object> PlugIns => ImmutableArray<object>.Empty;

        public bool IsFrameworkEnabled => true;

        public bool FormatOutput => false;

        public bool FormatCompileTimeCode { get; set; }

        public bool IsUserCodeTrusted => true;

        public string? ProjectPath => null;

        public string? TargetFramework => "net5.0";

        public string? Configuration => "Debug";

        public string ProjectDirectory { get; }

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