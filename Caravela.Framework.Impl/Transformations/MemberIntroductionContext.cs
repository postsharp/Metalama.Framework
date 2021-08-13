// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MemberIntroductionContext
    {
        public IServiceProvider ServiceProvider { get; }

        public UserDiagnosticSink DiagnosticSink { get; }

        public IntroductionNameProvider IntroductionNameProvider { get; }

        public ITemplateLexicalScopeProvider LexicalScopeProvider { get; }

        public ISyntaxFactory SyntaxFactory { get; }

        public MemberIntroductionContext(
            UserDiagnosticSink diagnosticSink,
            IntroductionNameProvider introductionNameProvider,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            ISyntaxFactory syntaxFactory,
            IServiceProvider serviceProvider )
        {
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScopeProvider = lexicalScopeProvider;
            this.SyntaxFactory = syntaxFactory;
            this.ServiceProvider = serviceProvider;
            this.IntroductionNameProvider = introductionNameProvider;
        }
    }
}