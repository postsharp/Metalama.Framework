// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal class TransformationContext
{
    public IServiceProvider ServiceProvider { get; }

    public UserDiagnosticSink DiagnosticSink { get; }

    public SyntaxGenerationContext SyntaxGenerationContext { get; }

    public SyntaxGeneratorWithContext SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;

    /// <summary>
    /// Gets the last compilation model of the linker input.
    /// </summary>
    public CompilationModel Compilation { get; }

    public ITemplateLexicalScopeProvider LexicalScopeProvider { get; }

    public TransformationContext(
        IServiceProvider serviceProvider,
        UserDiagnosticSink diagnosticSink,
        SyntaxGenerationContext syntaxGenerationContext,
        CompilationModel compilation,
        ITemplateLexicalScopeProvider lexicalScopeProvider )
    {
        this.ServiceProvider = serviceProvider;
        this.DiagnosticSink = diagnosticSink;
        this.SyntaxGenerationContext = syntaxGenerationContext;
        this.Compilation = compilation;
        this.LexicalScopeProvider = lexicalScopeProvider;
    }
}