// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class SyntaxGenerationContextFactory
{
    private readonly SemanticModelProvider _semanticModelProvider;
    private readonly SyntaxGenerationContext _nullOblivious;

    public SyntaxGenerationContext Default { get; }

    public SyntaxGenerationContextFactory( CompilationContext compilationContext )
    {
        this.Default = SyntaxGenerationContext.Create( compilationContext );
        this._nullOblivious = SyntaxGenerationContext.Create( compilationContext, isNullOblivious: true );
        this._semanticModelProvider = compilationContext.Compilation.GetSemanticModelProvider();
    }

    public SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );
        var nullableContext = semanticModel.GetNullableContext( node.SpanStart );
        var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

        return isNullOblivious ? this._nullOblivious : this.Default;
    }
}