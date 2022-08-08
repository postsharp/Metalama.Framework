// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

public class TestContext : IDisposable, ITempFileManager, IApplicationInfoProvider
{
    private static readonly IApplicationInfo _applicationInfo = new TestFrameworkApplicationInfo();
    private readonly ITempFileManager _backstageTempFileManager;

    public TestProjectOptions ProjectOptions { get; }

    public ServiceProvider ServiceProvider { get; }

    public InMemoryConfigurationManager ConfigurationManager { get; }

    public TestContext( TestProjectOptions? projectOptions = null, Func<ServiceProvider, ServiceProvider>? addServices = null )
    {
        this._backstageTempFileManager = (ITempFileManager) BackstageServiceFactory.ServiceProvider.GetService( typeof(ITempFileManager) );

        this.ProjectOptions = projectOptions ?? new TestProjectOptions();

        var backstageServiceProvider = new BackstageServiceProvider( this );
        this.ConfigurationManager = new InMemoryConfigurationManager( backstageServiceProvider );

        this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( backstageServiceProvider )
            .WithService( this.ProjectOptions )
            .WithProjectScopedServices( TestCompilationFactory.GetMetadataReferences() )
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
        if ( subdirectory == ReferenceAssemblyLocator.TempDirectory )
        {
            return this._backstageTempFileManager.GetTempDirectory( subdirectory, cleanUpStrategy, guid );
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
            if ( serviceType == typeof(ITempFileManager) || serviceType == typeof(IApplicationInfoProvider) )
            {
                return this._context;
            }
            else if ( serviceType == typeof(IProjectOptions) )
            {
                return this._context.ProjectOptions;
            }
            else if ( serviceType == typeof(IConfigurationManager) )
            {
                return this._context.ConfigurationManager;
            }
            else
            {
                return null;
            }
        }
    }

    public IApplicationInfo CurrentApplication => _applicationInfo;
}