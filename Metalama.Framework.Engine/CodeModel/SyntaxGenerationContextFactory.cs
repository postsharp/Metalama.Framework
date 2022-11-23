// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

internal class SyntaxGenerationContextFactory
{
    private readonly SemanticModelProvider _semanticModelProvider;

    public Compilation Compilation { get; }

    public SyntaxGenerationContext Default { get; }

    public SyntaxGenerationContext NullOblivious { get; }

    public SyntaxGenerationContextFactory( CompilationContext compilationContext )
    {
        this.Compilation = compilationContext.Compilation;
        this.Default = SyntaxGenerationContext.Create( compilationContext );
        this.NullOblivious = SyntaxGenerationContext.Create( compilationContext, isNullOblivious: true );
        this._semanticModelProvider = compilationContext.Compilation.GetSemanticModelProvider();
    }

    public SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );
        var nullableContext = semanticModel.GetNullableContext( node.SpanStart );
        var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

        return isNullOblivious ? this.NullOblivious : this.Default;
    }
}