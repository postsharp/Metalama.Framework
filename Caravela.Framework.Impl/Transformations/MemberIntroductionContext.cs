// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MemberIntroductionContext
    {
        public DiagnosticSink DiagnosticSink { get; }

        public IntroductionNameProvider IntroductionNameProvider { get; }

        public ITemplateExpansionLexicalScope LexicalScope { get; }

        public ProceedImplementationFactory ProceedImplementationFactory { get; }

        public MemberIntroductionContext( DiagnosticSink diagnosticSink, IntroductionNameProvider introductionNameProvider, ITemplateExpansionLexicalScope lexicalScope, ProceedImplementationFactory proceedImplementationFactory )
        {
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScope = lexicalScope;
            this.IntroductionNameProvider = introductionNameProvider;
            this.ProceedImplementationFactory = proceedImplementationFactory;
        }
    }
}
