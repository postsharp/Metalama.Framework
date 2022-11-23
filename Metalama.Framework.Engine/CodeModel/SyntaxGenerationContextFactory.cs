// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

    public SyntaxGenerationContextFactory( CompilationServices compilationServices )
    {
        this.Compilation = compilationServices.Compilation;
        this.Default = SyntaxGenerationContext.Create( compilationServices );
        this.NullOblivious = SyntaxGenerationContext.Create( compilationServices, isNullOblivious: true );
        this._semanticModelProvider = compilationServices.Compilation.GetSemanticModelProvider();
    }

    public SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );
        var nullableContext = semanticModel.GetNullableContext( node.SpanStart );
        var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

        return isNullOblivious ? this.NullOblivious : this.Default;
    }
}