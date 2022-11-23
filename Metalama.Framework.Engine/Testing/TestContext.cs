// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
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

    public ProjectServiceProvider ServiceProvider { get; }

    public TestContext(
        TestProjectOptions projectOptions,
        IEnumerable<MetadataReference>? metalamaReferences = null,
        TestServiceFactory? mockFactory = null )
    {
        this.ProjectOptions = projectOptions;
        this._backstageTempFileManager = BackstageServiceFactory.ServiceProvider.GetRequiredBackstageService<ITempFileManager>();
        
        var backstageServices = ServiceProvider<IBackstageService>.Empty.WithNextProvider( BackstageServiceFactory.ServiceProvider )
            .WithService( this);
        this._configurationManager = new InMemoryConfigurationManager( backstageServices );

        if ( mockFactory != null )
        {
            backstageServices = backstageServices.WithServices( mockFactory.BackstageServices.GetAdditionalServices( backstageServices ) );
        }
        
        var serviceProvider = ServiceProviderFactory.GetServiceProvider( backstageServices, mockFactory?.GlobalServices );
        
        this.ServiceProvider = serviceProvider
            .WithProjectScopedServices( projectOptions, metalamaReferences ?? TestCompilationFactory.GetMetadataReferences(), mockFactory?.ProjectServices );

    }

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
    
    public IApplicationInfo CurrentApplication => _applicationInfo;
}