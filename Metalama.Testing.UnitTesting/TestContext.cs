// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Testing.UnitTesting;

/// <summary>
/// A context in which a Metalama unit test can run, configured with most required Metalama services and optionally some mocks.
/// </summary>
public class TestContext : IDisposable, ITempFileManager, IApplicationInfoProvider, IDateTimeProvider
{
    private static readonly IApplicationInfo _applicationInfo = new TestApiApplicationInfo();
    private readonly ITempFileManager _backstageTempFileManager;
    private readonly bool _isRoot;

    // We keep the domain in a strongbox so that we share domain instances with TestContext instances created with With* method.
    private readonly StrongBox<UnloadableCompileTimeDomain?> _domain;

    private volatile CancellationTokenSource? _timeout;
    private CancellationTokenRegistration? _timeoutAction;

    internal TestProjectOptions ProjectOptions { get; }

    /// <summary>
    /// Gets the <see cref="ProjectServiceProvider"/> for the current context.
    /// </summary>
    public ProjectServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets a <see cref="CancellationToken"/> used to cancel the test in case of timeout. The timeout period is defined
    /// by the <see cref="TestContextOptions.Timeout"/> option.
    /// </summary>
    public CancellationToken CancellationToken
    {
        get
        {
            if ( this._timeout == null )
            {
                if ( Interlocked.CompareExchange( ref this._timeout, new CancellationTokenSource( TimeSpan.FromSeconds( 120 ) ), null ) == null )
                {
                    this._timeoutAction = this._timeout.Token.Register(
                        () => this.ServiceProvider.GetLoggerFactory().GetLogger( "Test" ).Error?.Log( "Test timeout. Cancelling." ) );
                }
            }

            return this._timeout.Token;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestContext"/> class. Tests typically
    /// do not call this constructor directly, but instead the <see cref="UnitTestClass.CreateTestContext(IAdditionalServiceCollection)"/>
    /// method.
    /// </summary>
    public TestContext(
        TestContextOptions contextOptions,
        IAdditionalServiceCollection? additionalServices = null )
    {
        this._domain = new StrongBox<UnloadableCompileTimeDomain?>();
        this._isRoot = true;

        this.ProjectOptions = new TestProjectOptions( contextOptions );
        this._backstageTempFileManager = BackstageServiceFactory.ServiceProvider.GetRequiredBackstageService<ITempFileManager>();

        var platformInfo = BackstageServiceFactory.ServiceProvider.GetRequiredBackstageService<IPlatformInfo>();

        // We intentionally replace (override) backstage services by ours.
        var backstageServices = ServiceProvider<IBackstageService>.Empty
            .WithService( this )
            .WithService( platformInfo );

        backstageServices = backstageServices.WithService( new InMemoryConfigurationManager( backstageServices ), true );

        var typedAdditionalServices = (AdditionalServiceCollection?) additionalServices ?? new AdditionalServiceCollection();
        typedAdditionalServices.GlobalServices.Add( sp => sp.TryWithService<IGlobalOptions>( _ => new TestGlobalOptions() ) );

        backstageServices = typedAdditionalServices.BackstageServices.Build( backstageServices );

        var serviceProvider = ServiceProviderFactory.GetServiceProvider( backstageServices, typedAdditionalServices );

        this.ServiceProvider = serviceProvider
            .WithProjectScopedServices( this.ProjectOptions, contextOptions.References );
    }

    private TestContext( TestContext prototype, IEnumerable<MetadataReference> newReferences )
    {
        this._domain = prototype._domain;
        this.ProjectOptions = prototype.ProjectOptions;
        this._backstageTempFileManager = prototype._backstageTempFileManager;
        this.ServiceProvider = prototype.ServiceProvider.Global.Underlying.WithProjectScopedServices( this.ProjectOptions, newReferences );
    }

    public TestContext WithReferences( IEnumerable<MetadataReference> newReferences ) => new( this, newReferences );

    /// <summary>
    /// Creates an <see cref="ICompilation"/> made of a single source file.
    /// </summary>
    /// <param name="code">Source code.</param>
    /// <param name="dependentCode">Source code of another assembly added as a reference to the
    /// returned assembly. Optional.</param>
    /// <param name="ignoreErrors">Determines whether compilation errors should be ignored.
    /// Optional.</param>
    /// <param name="additionalReferences">Additional set of <see cref="MetadataReference"/>
    /// added to the compilation.</param>
    /// <param name="name">Name of the assembly.</param>
    /// <param name="addMetalamaReferences">Determines if Metalama assemblies should be added
    /// as references to the compilation. Optional. The default value is <c>true</c>.</param>
    /// <returns></returns>
    public ICompilation CreateCompilation(
        string code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
        => this.CreateCompilation(
            new Dictionary<string, string> { { "test.cs", code } },
            dependentCode,
            ignoreErrors,
            additionalReferences,
            name,
            addMetalamaReferences );

    /// <summary>
    /// Creates an <see cref="ICompilation"/> made of several source files.
    /// </summary>
    /// <param name="code">Source code. The key of the dictionary item is the file name, the value is
    /// the source code.</param>
    /// <param name="dependentCode">Source code of another assembly added as a reference to the
    /// returned assembly. Optional.</param>
    /// <param name="ignoreErrors">Determines whether compilation errors should be ignored.
    /// Optional.</param>
    /// <param name="additionalReferences">Additional set of <see cref="MetadataReference"/>
    /// added to the compilation.</param>
    /// <param name="name">Name of the assembly.</param>
    /// <param name="addMetalamaReferences">Determines if Metalama assemblies should be added
    /// as references to the compilation. Optional. The default value is <c>true</c>.</param>
    public ICompilation CreateCompilation(
        IReadOnlyDictionary<string, string> code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
    {
        var allAdditionalReferences = ImmutableArray<MetadataReference>.Empty;

        if ( !this.ProjectOptions.AdditionalAssemblies.IsDefaultOrEmpty )
        {
            allAdditionalReferences = allAdditionalReferences.AddRange(
                this.ProjectOptions.AdditionalAssemblies.Select( a => MetadataReference.CreateFromFile( a.Location ) ) );
        }

        if ( additionalReferences != null )
        {
            allAdditionalReferences = allAdditionalReferences.AddRange( additionalReferences );
        }

        var roslynCompilation = TestCompilationFactory.CreateCSharpCompilation(
            code,
            dependentCode,
            ignoreErrors,
            allAdditionalReferences,
            name,
            addMetalamaReferences );

        return CompilationModel.CreateInitialInstance(
            new ProjectModel( roslynCompilation, this.ServiceProvider ),
            roslynCompilation );
    }

    /// <summary>
    /// Creates an <see cref="ICompilation"/> from a <see cref="Compilation"/>.
    /// </summary>
    public ICompilation CreateCompilation( Compilation compilation )
        => CompilationModel.CreateInitialInstance(
            new ProjectModel( compilation, this.ServiceProvider ),
            compilation );

    internal CompilationModel CreateCompilationModel(
        string code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
        => (CompilationModel) this.CreateCompilation( code, dependentCode, ignoreErrors, additionalReferences, name, addMetalamaReferences );

    internal CompilationModel CreateCompilationModel(
        IReadOnlyDictionary<string, string> code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
        => (CompilationModel) this.CreateCompilation( code, dependentCode, ignoreErrors, additionalReferences, name, addMetalamaReferences );

    internal CompilationModel CreateCompilationModel( Compilation compilation ) => (CompilationModel) this.CreateCompilation( compilation );

    private UnloadableCompileTimeDomain CreateDomain()
    {
        var domain = new UnloadableCompileTimeDomain();

        // Prevents the ProjectOptions from being disposed while the domain is in used, because the domain typically
        // locks files in the directory created by ProjectOptions.
        this.ProjectOptions.AddFileLocker();

#if NET5_0_OR_GREATER
        domain.Unloaded += this.ProjectOptions.RemoveFileLocker;
#endif

        return domain;
    }

    internal UnloadableCompileTimeDomain Domain => this._domain.Value ??= this.CreateDomain();

    string ITempFileManager.GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid )
    {
        if ( subdirectory.StartsWith( TempDirectories.AssemblyLocator, StringComparison.Ordinal ) )
        {
            return this._backstageTempFileManager.GetTempDirectory( subdirectory, cleanUpStrategy, this.GetType().Module.ModuleVersionId );
        }
        else
        {
            var directory = Path.Combine( this.ProjectOptions.BaseDirectory, subdirectory, guid?.ToString() ?? "" );

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            return directory;
        }
    }

    DateTime IDateTimeProvider.Now => DateTime.Now;

    public virtual void Dispose()
    {
        if ( this._isRoot )
        {
            this.ProjectOptions.Dispose();
            this._domain.Value?.Dispose();
            this._timeout?.Dispose();
            this._timeoutAction?.Dispose();
        }
    }

    IApplicationInfo IApplicationInfoProvider.CurrentApplication => _applicationInfo;
}