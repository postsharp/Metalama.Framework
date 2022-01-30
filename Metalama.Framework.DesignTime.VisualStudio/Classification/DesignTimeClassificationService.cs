// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Classification;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio;

internal class DesignTimeClassificationService : IClassificationService
{
    private readonly ClassificationService _classificationService;

    public DesignTimeClassificationService() : this(
        VisualStudioServiceProviderFactory.GetServiceProvider().WithProjectScopedServices( Enumerable.Empty<MetadataReference>() ) ) { }

    internal DesignTimeClassificationService( ServiceProvider serviceProvider )
    {
        this._classificationService = new ClassificationService( serviceProvider );
    }

    public bool ContainsCompileTimeCode( SyntaxNode syntaxRoot ) => this._classificationService.ContainsCompileTimeCode( syntaxRoot );

    public IDesignTimeClassifiedTextCollection GetClassifiedTextSpans( SemanticModel model, CancellationToken cancellationToken )
    {
        return new DesignTimeClassifiedTextSpansCollection( this._classificationService.GetClassifiedTextSpans( model, cancellationToken ) );
    }
}