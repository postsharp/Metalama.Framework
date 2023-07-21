// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Context for code transformation's syntax node evaluation.
    /// </summary>
    internal sealed class InsertStatementTransformationContext : TransformationContext
    {
        public InsertStatementTransformationContext(
            ProjectServiceProvider serviceProvider,
            UserDiagnosticSink diagnosticSink,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            CompilationModel compilation ) : base( serviceProvider, diagnosticSink, syntaxGenerationContext, compilation, lexicalScopeProvider ) { }
    }
}