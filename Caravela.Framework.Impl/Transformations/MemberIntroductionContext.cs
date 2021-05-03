// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using System;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MemberIntroductionContext
    {
        public IServiceProvider ServiceProvider { get; }

        public DiagnosticSink DiagnosticSink { get; }

        public IntroductionNameProvider IntroductionNameProvider { get; }

        public TemplateLexicalScope LexicalScope { get; }

        public ISyntaxFactory SyntaxFactory { get; }

        public MemberIntroductionContext(
            DiagnosticSink diagnosticSink,
            IntroductionNameProvider introductionNameProvider,
            TemplateLexicalScope lexicalScope,
            ISyntaxFactory syntaxFactory,
            IServiceProvider serviceProvider )
        {
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScope = lexicalScope;
            this.SyntaxFactory = syntaxFactory;
            this.ServiceProvider = serviceProvider;
            this.IntroductionNameProvider = introductionNameProvider;
        }
    }
}