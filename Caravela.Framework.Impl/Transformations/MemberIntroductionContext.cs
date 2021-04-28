// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MemberIntroductionContext
    {
        public DiagnosticSink DiagnosticSink { get; }

        public IntroductionNameProvider IntroductionNameProvider { get; }

        public TemplateExpansionLexicalScope LexicalScope { get; }
        
        
        public ISyntaxFactory SyntaxFactory { get; }

        public MemberIntroductionContext(
            DiagnosticSink diagnosticSink,
            IntroductionNameProvider introductionNameProvider,
            TemplateExpansionLexicalScope lexicalScope,
            ISyntaxFactory syntaxFactory )
        {
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScope = lexicalScope;
            this.SyntaxFactory = syntaxFactory;
            this.IntroductionNameProvider = introductionNameProvider;
        }
    }
}