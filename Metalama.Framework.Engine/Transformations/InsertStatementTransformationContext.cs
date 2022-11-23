// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Context for code transformation's syntax node evaluation.
    /// </summary>
    internal class InsertStatementTransformationContext : TransformationContext
    {
        public InsertStatementTransformationContext(
            UserDiagnosticSink diagnosticSink,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            CompilationModel compilation ) : base( diagnosticSink, syntaxGenerationContext, compilation, lexicalScopeProvider ) { }
    }
}