// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.Transformations;

internal class TransformationContext
{
    public ProjectServiceProvider ServiceProvider { get; }

    public UserDiagnosticSink DiagnosticSink { get; }

    public SyntaxGenerationContext SyntaxGenerationContext { get; }

    public SyntaxGeneratorWithContext SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;

    /// <summary>
    /// Gets the last compilation model of the linker input.
    /// </summary>
    public CompilationModel Compilation { get; }

    public ITemplateLexicalScopeProvider LexicalScopeProvider { get; }

    public TransformationContext(
        ProjectServiceProvider serviceProvider,
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