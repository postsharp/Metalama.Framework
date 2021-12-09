// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class MemberIntroductionContext
    {
        public IServiceProvider ServiceProvider { get; }

        public UserDiagnosticSink DiagnosticSink { get; }

        public IntroductionNameProvider IntroductionNameProvider { get; }

        public ITemplateLexicalScopeProvider LexicalScopeProvider { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public OurSyntaxGenerator SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;

        public MemberIntroductionContext(
            UserDiagnosticSink diagnosticSink,
            IntroductionNameProvider introductionNameProvider,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IServiceProvider serviceProvider )
        {
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScopeProvider = lexicalScopeProvider;
            this.ServiceProvider = serviceProvider;
            this.IntroductionNameProvider = introductionNameProvider;
            this.SyntaxGenerationContext = syntaxGenerationContext;
        }
    }
}