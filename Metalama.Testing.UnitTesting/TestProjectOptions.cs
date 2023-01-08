// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Metalama.Testing.UnitTesting
{
    /// <summary>
    /// An implementation of <see cref="IProjectOptions"/> that can be used in tests.
    /// </summary>
    internal sealed class TestProjectOptions : DefaultProjectOptions, IDisposable
    {
        private readonly ImmutableDictionary<string, string> _properties;
        private readonly Lazy<string> _baseDirectory;
        private readonly Lazy<string> _projectDirectory;
        private int _fileLockers;

        public TestProjectOptions( TestContextOptions contextOptions )
        {
            this.PlugIns = contextOptions.PlugIns.IsDefault ? ImmutableArray<object>.Empty : contextOptions.PlugIns;

            this._properties = contextOptions.Properties;

            // We don't use the backstage TempFileManager because it would generate paths that are too long.
            var baseDirectory = Path.Combine( Path.GetTempPath(), "Metalama", "Tests", Guid.NewGuid().ToString() );
            this._baseDirectory = CreateDirectoryLazy( baseDirectory );

            var projectDirectory = Path.Combine( baseDirectory, "Project" );
            this._projectDirectory = CreateDirectoryLazy( projectDirectory );

            this.FormatOutput = contextOptions.FormatOutput;
            this.FormatCompileTimeCode = contextOptions.FormatCompileTimeCode;
            this.AdditionalAssemblies = contextOptions.AdditionalAssemblies;
            this.RequireOrderedAspects = contextOptions.RequireOrderedAspects;

            if ( contextOptions.HasSourceGeneratorTouchFile )
            {
                this.SourceGeneratorTouchFile = Path.Combine( baseDirectory, "SourceGeneratorTouchFile.txt" );
            }

            if ( contextOptions.HasBuildTouchFile )
            {
                this.BuildTouchFile = Path.Combine( baseDirectory, "BuildTouchFile.txt" );
            }
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

        public override string? SourceGeneratorTouchFile { get; }

        public string ProjectDirectory => this._projectDirectory.Value;

        public override bool IsTest => true;

        public override string? BuildTouchFile { get; }

        public override bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._properties.TryGetValue( name, out value );

        public void AddFileLocker()
        {
            Interlocked.Increment( ref this._fileLockers );
        }

        public void RemoveFileLocker()
        {
            if ( Interlocked.Decrement( ref this._fileLockers ) == 0 )
            {
                this.Dispose();
            }
        }

        public void Dispose()
        {
            if ( this._fileLockers == 0 )
            {
                if ( Directory.Exists( this.BaseDirectory ) )
                {
                    try
                    {
                        RetryHelper.Retry( () => Directory.Delete( this.BaseDirectory, true ) );
                    }
                    catch ( DirectoryNotFoundException ) { }
                }
            }
        }
    }
}