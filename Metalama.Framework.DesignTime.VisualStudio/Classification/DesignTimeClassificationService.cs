// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Metalama.Framework.DesignTime.VisualStudio.Classification;

internal class DesignTimeClassificationService : IClassificationService
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMetalamaProjectClassifier _projectClassifier;
    private readonly WeakCache<MSBuildProjectOptions, ClassificationService> _projectClassificationServices = new();

    private readonly MSBuildProjectOptionsFactory _msBuildProjectOptionsFactory = new( new[] { MSBuildPropertyNames.MetalamaCompileTimePackages } );

    public DesignTimeClassificationService() : this( DesignTimeServiceProviderFactory.GetServiceProvider( true ) ) { }

    public DesignTimeClassificationService( ServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._projectClassifier = serviceProvider.GetRequiredService<IMetalamaProjectClassifier>();
    }

    public bool ContainsCompileTimeCode( SyntaxNode syntaxRoot ) => ClassificationService.ContainsCompileTimeCode( syntaxRoot );

    public IDesignTimeClassifiedTextCollection GetClassifiedTextSpans(
        SemanticModel semanticModel,
        AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider,
        CancellationToken cancellationToken )
    {
        if ( semanticModel.Compilation.ExternalReferences.IsDefaultOrEmpty
             || !this._projectClassifier.IsMetalamaEnabled( semanticModel.Compilation ) )
        {
            // Do not return anything if the compilation is not initialized or is not a Metalama project.
            return EmptyDesignTimeClassifiedTextCollection.Instance;
        }

        var projectOptions = this._msBuildProjectOptionsFactory.GetInstance( analyzerConfigOptionsProvider );

        var classificationService = this._projectClassificationServices.GetOrAdd( projectOptions, this.CreateClassificationService );

        return new DesignTimeClassifiedTextSpansCollection( classificationService.GetClassifiedTextSpans( semanticModel, cancellationToken ) );
    }

    private ClassificationService CreateClassificationService( MSBuildProjectOptions options )
    {
        return new ClassificationService( this._serviceProvider.WithProjectScopedServices( options, Array.Empty<MetadataReference>() ) );
    }
}