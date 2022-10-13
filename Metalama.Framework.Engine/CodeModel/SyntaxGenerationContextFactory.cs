// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

internal class SyntaxGenerationContextFactory : IService
{
    private readonly SemanticModelProvider _semanticModelProvider;

    public Compilation Compilation { get; }

    public SyntaxGenerationContext Default { get; }

    public SyntaxGenerationContext NullOblivious { get; }

    public SyntaxGenerationContextFactory( Compilation compilation, IServiceProvider serviceProvider )
    {
        this.Compilation = compilation;
        this.Default = SyntaxGenerationContext.Create( serviceProvider, compilation );
        this.NullOblivious = SyntaxGenerationContext.Create( serviceProvider, compilation, isNullOblivious: true );
        this._semanticModelProvider = compilation.GetSemanticModelProvider();
    }

    public SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );
        var nullableContext = semanticModel.GetNullableContext( node.SpanStart );
        var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

        return isNullOblivious ? this.NullOblivious : this.Default;
    }
}