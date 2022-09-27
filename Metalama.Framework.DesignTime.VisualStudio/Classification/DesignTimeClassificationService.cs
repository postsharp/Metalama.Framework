// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.Classification;

internal class DesignTimeClassificationService : IClassificationService
{
    private readonly ClassificationService _classificationService;
    private readonly IMetalamaProjectClassifier _projectClassifier;

    public DesignTimeClassificationService() : this(
        VsServiceProviderFactory.GetServiceProvider().WithProjectScopedServices( Enumerable.Empty<MetadataReference>() ) ) { }

    private DesignTimeClassificationService( ServiceProvider serviceProvider )
    {
        this._classificationService = new ClassificationService( serviceProvider );
        this._projectClassifier = serviceProvider.GetRequiredService<IMetalamaProjectClassifier>();
    }

    public bool ContainsCompileTimeCode( SyntaxNode syntaxRoot ) => ClassificationService.ContainsCompileTimeCode( syntaxRoot );

    public IDesignTimeClassifiedTextCollection GetClassifiedTextSpans( SemanticModel model, CancellationToken cancellationToken )
    {
        if ( model.Compilation.ExternalReferences.IsDefaultOrEmpty
             || !this._projectClassifier.IsMetalamaEnabled( model.Compilation ) )
        {
            // Do not return anything if the compilation is not initialized or is not a Metalama project.
            return EmptyDesignTimeClassifiedTextCollection.Instance;
        }

        return new DesignTimeClassifiedTextSpansCollection( this._classificationService.GetClassifiedTextSpans( model, cancellationToken ) );
    }
}