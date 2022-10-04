// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Engine.Testing;

public class TestContext : IDisposable, ITempFileManager, IApplicationInfoProvider, IDateTimeProvider
{
    private static readonly IApplicationInfo _applicationInfo = new TestFrameworkApplicationInfo();
    private readonly ITempFileManager _backstageTempFileManager;
    private readonly InMemoryConfigurationManager _configurationManager;

    public TestProjectOptions ProjectOptions { get; }

    public ServiceProvider ServiceProvider { get; }

    public TestContext( TestProjectOptions projectOptions, Func<ServiceProvider, ServiceProvider>? addServices = null )
    {
        this._backstageTempFileManager = BackstageServiceFactory.ServiceProvider.GetRequiredBackstageService<ITempFileManager>();

        var backstageServiceProvider = new BackstageServiceProvider( this );
        this._configurationManager = new InMemoryConfigurationManager( backstageServiceProvider );

        this.ProjectOptions = projectOptions;

        this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( backstageServiceProvider )
            .WithService( new TestMarkerService() )
            .WithProjectScopedServices( projectOptions, TestCompilationFactory.GetMetadataReferences() )
            .WithMark( ServiceProviderMark.Test );

        if ( addServices != null )
        {
            this.ServiceProvider = addServices( this.ServiceProvider );
        }
    }

    public ICompilation CreateCompilation(
        string code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null )
        => this.CreateCompilationModel( code, dependentCode, ignoreErrors, additionalReferences, name );

    public CompilationModel CreateCompilationModel(
        string code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
        => this.CreateCompilationModel(
            new Dictionary<string, string> { { "test.cs", code } },
            dependentCode,
            ignoreErrors,
            additionalReferences,
            name,
            addMetalamaReferences );

    public CompilationModel CreateCompilationModel(
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

        var roslynCompilation = TestBase.CreateCSharpCompilation( code, dependentCode, ignoreErrors, allAdditionalReferences, name, addMetalamaReferences );

        return CompilationModel.CreateInitialInstance(
            new ProjectModel( roslynCompilation, this.ServiceProvider ),
            roslynCompilation );
    }

    internal CompilationModel CreateCompilationModel( Compilation compilation )
        => CompilationModel.CreateInitialInstance(
            new ProjectModel( compilation, this.ServiceProvider ),
            compilation );

    string ITempFileManager.GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid )
    {
        if ( subdirectory.StartsWith( ReferenceAssemblyLocator.TempDirectory, StringComparison.Ordinal ) )
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

    private class BackstageServiceProvider : IServiceProvider
    {
        private readonly TestContext _context;

        public BackstageServiceProvider( TestContext context )
        {
            this._context = context;
        }

        object? IServiceProvider.GetService( Type serviceType )
        {
            if ( serviceType == typeof(ITempFileManager) || serviceType == typeof(IApplicationInfoProvider) || serviceType == typeof(IDateTimeProvider) )
            {
                return this._context;
            }
            else if ( serviceType == typeof(IProjectOptions) )
            {
                return this._context.ProjectOptions;
            }
            else if ( serviceType == typeof(IConfigurationManager) )
            {
                return this._context._configurationManager;
            }
            else if ( typeof(IBackstageService).IsAssignableFrom( serviceType ) )
            {
                return BackstageServiceFactory.ServiceProvider.GetService( serviceType );
            }
            else
            {
                return null;
            }
        }
    }

    public IApplicationInfo CurrentApplication => _applicationInfo;
}