// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.Api.Options;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Metalama.Testing.Api;

/// <summary>
/// A context in which a Metalama unit test can run, configured with most required Metalama services and optionally some mocks.
/// </summary>
public class TestContext : IDisposable, ITempFileManager, IApplicationInfoProvider, IDateTimeProvider
{
    private static readonly IApplicationInfo _applicationInfo = new TestApiApplicationInfo();
    private readonly ITempFileManager _backstageTempFileManager;

    internal TestProjectOptions ProjectOptions { get; }

    public ProjectServiceProvider ServiceProvider { get; }

    public TestContext(
        TestContextOptions contextOptions,
        IEnumerable<MetadataReference>? metalamaReferences = null,
        IAdditionalServiceCollection? additionalServices = null )
    {
        this.ProjectOptions = new TestProjectOptions( contextOptions );
        this._backstageTempFileManager = BackstageServiceFactory.ServiceProvider.GetRequiredBackstageService<ITempFileManager>();

        // We intentionally replace (override) backstage services by ours.
        var backstageServices = ServiceProvider<IBackstageService>.Empty.WithNextProvider( BackstageServiceFactory.ServiceProvider )
            .WithService( this, true );

        backstageServices = backstageServices.WithService( new InMemoryConfigurationManager( backstageServices ), true );

        var typedAdditionalServices = (AdditionalServiceCollection?) additionalServices ?? new AdditionalServiceCollection();
        typedAdditionalServices.GlobalServices.Add( sp => sp.TryWithService<IGlobalOptions>( _ => new TestGlobalOptions() ) );

        backstageServices = typedAdditionalServices.BackstageServices.ServiceProvider.WithNextProvider( backstageServices );

        var serviceProvider = ServiceProviderFactory.GetServiceProvider( backstageServices, typedAdditionalServices );

        this.ServiceProvider = serviceProvider
            .WithProjectScopedServices( this.ProjectOptions, metalamaReferences ?? TestCompilationFactory.GetMetadataReferences() );
    }

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

    public ICompilation CreateCompilation( Compilation compilation )
        => CompilationModel.CreateInitialInstance(
            new ProjectModel( compilation, this.ServiceProvider ),
            compilation );

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

    public void Dispose()
    {
        this.ProjectOptions.Dispose();
    }

    public IApplicationInfo CurrentApplication => _applicationInfo;
}