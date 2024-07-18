// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Metalama.Testing.UnitTesting;

/// <summary>
/// An implementation of <see cref="IProjectOptions"/> that can be used in tests.
/// </summary>
internal sealed class TestProjectOptions : DefaultProjectOptions, IDisposable
{
    private readonly ImmutableDictionary<string, string> _properties;
    private readonly Lazy<string> _baseDirectory;
    private readonly Lazy<string> _projectDirectory;
    private int _fileLockers;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestProjectOptions"/> class from
    /// a prototype <see cref="TestContextOptions"/>, allowing to override some properties.
    /// </summary>
    public TestProjectOptions( TestProjectOptions prototype, CodeFormattingOptions? codeFormattingOptions = null )
    {
        this.ProjectName = prototype.ProjectName;
        this._properties = prototype._properties;
        this._baseDirectory = prototype._baseDirectory;
        this._projectDirectory = prototype._projectDirectory;
        this.CodeFormattingOptions = codeFormattingOptions ?? prototype.CodeFormattingOptions;
        this.FormatCompileTimeCode = prototype.FormatCompileTimeCode;
        this.AdditionalAssemblies = prototype.AdditionalAssemblies;
        this.RequireOrderedAspects = prototype.RequireOrderedAspects;
        this.RoslynIsCompileTimeOnly = prototype.RoslynIsCompileTimeOnly;
        this.SourceGeneratorTouchFile = prototype.SourceGeneratorTouchFile;
        this.BuildTouchFile = prototype.BuildTouchFile;
        this.DomainObserver = new DomainObserverImpl( this );
    }

    public TestProjectOptions( TestContextOptions contextOptions )
    {
        this.ProjectName = contextOptions.ProjectName;
        this._properties = contextOptions.Properties;

        // We don't use the backstage TempFileManager because it would generate paths that are too long.
        var baseDirectory = Path.Combine( MetalamaPathUtilities.GetTempPath(), "Metalama", "Tests", Guid.NewGuid().ToString() );

        if ( contextOptions.TempPathLength.HasValue )
        {
            var currentLenght = baseDirectory.Length;
            var remainingLength = contextOptions.TempPathLength.Value - currentLenght - 1;

            switch ( remainingLength )
            {
                case < 0:
                    throw new InvalidOperationException( "The base path is too short." );

                case > 0:
                    baseDirectory += '_' + new string( 'x', remainingLength );

                    break;
            }
        }

        this._baseDirectory = CreateDirectoryLazy( baseDirectory );

        var projectDirectory = Path.Combine( baseDirectory, "Project" );
        this._projectDirectory = CreateDirectoryLazy( projectDirectory );

        this.CodeFormattingOptions = contextOptions.CodeFormattingOptions;
        this.FormatCompileTimeCode = contextOptions.FormatCompileTimeCode;
        this.AdditionalAssemblies = contextOptions.AdditionalAssemblies;
        this.RequireOrderedAspects = contextOptions.RequireOrderedAspects;
        this.RoslynIsCompileTimeOnly = contextOptions.RoslynIsCompileTimeOnly;

        if ( contextOptions.HasSourceGeneratorTouchFile )
        {
            this.SourceGeneratorTouchFile = Path.Combine( baseDirectory, "SourceGeneratorTouchFile.txt" );
        }

        if ( contextOptions.HasBuildTouchFile )
        {
            this.BuildTouchFile = Path.Combine( baseDirectory, "BuildTouchFile.txt" );
        }

        this.DomainObserver = new DomainObserverImpl( this );
    }

    internal ICompileTimeDomainObserver DomainObserver { get; }

    private static Lazy<string> CreateDirectoryLazy( string path )
        => new(
            () =>
            {
                Directory.CreateDirectory( path );

                return path;
            } );

    public override string? ProjectName { get; }

    public string BaseDirectory => this._baseDirectory.Value;

    public override CodeFormattingOptions CodeFormattingOptions { get; }

    public override bool FormatCompileTimeCode { get; }

    public override bool RequireOrderedAspects { get; }

    public ImmutableArray<Assembly> AdditionalAssemblies { get; }

    public override string? SourceGeneratorTouchFile { get; }

    public string ProjectDirectory => this._projectDirectory.Value;

    public override bool IsTest => true;

    public override string? BuildTouchFile { get; }

    public override bool RoslynIsCompileTimeOnly { get; }

    public override bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._properties.TryGetValue( name, out value );

    private void AddFileLocker() => Interlocked.Increment( ref this._fileLockers );

    private void RemoveFileLocker()
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
                catch ( UnauthorizedAccessException ) { }
            }
        }
    }

    private sealed class DomainObserverImpl : ICompileTimeDomainObserver
    {
        private readonly TestProjectOptions _parent;

        public DomainObserverImpl( TestProjectOptions parent )
        {
            this._parent = parent;
        }

        void ICompileTimeDomainObserver.OnDomainCreated( CompileTimeDomain domain ) => this._parent.AddFileLocker();

        void ICompileTimeDomainObserver.OnDomainUnloaded( CompileTimeDomain domain ) => this._parent.RemoveFileLocker();
    }
}