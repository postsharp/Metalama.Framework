// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

internal class SyntaxGenerationContextFactory : IService
{
    public Compilation Compilation { get; }

    public SyntaxGenerationContext Default { get; }

    public SyntaxGenerationContext NullOblivious { get; }

    public SyntaxGenerationContextFactory( Compilation compilation, IServiceProvider serviceProvider )
    {
        this.Compilation = compilation;
        this.Default = SyntaxGenerationContext.Create( serviceProvider, compilation );
        this.NullOblivious = SyntaxGenerationContext.Create( serviceProvider, compilation, isNullOblivious: true );
    }

    public SyntaxGenerationContext Get( SyntaxNode node )
    {
        var semanticModel = this.Compilation.GetSemanticModel( node.SyntaxTree );
        var nullableContext = semanticModel.GetNullableContext( node.SpanStart );
        var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

        return isNullOblivious ? this.NullOblivious : this.Default;
    }
}