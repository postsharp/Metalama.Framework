// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Context for code transformation's syntax node evaluation.
    /// </summary>
    internal readonly struct InsertStatementTransformationContext
    {
        public IServiceProvider ServiceProvider { get; }

        public UserDiagnosticSink DiagnosticSink { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public ITemplateLexicalScopeProvider LexicalScopeProvider { get; }

        public InsertStatementTransformationContext(
            UserDiagnosticSink diagnosticSink,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IServiceProvider serviceProvider )
        {
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScopeProvider = lexicalScopeProvider;
            this.SyntaxGenerationContext = syntaxGenerationContext;
            this.ServiceProvider = serviceProvider;
        }
    }
}