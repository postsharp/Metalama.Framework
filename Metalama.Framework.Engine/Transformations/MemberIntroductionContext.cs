// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class MemberIntroductionContext : TransformationContext
    {
        public IntroductionNameProvider IntroductionNameProvider { get; }

        public AspectReferenceSyntaxProvider AspectReferenceSyntaxProvider { get; }

        public MemberIntroductionContext(
            UserDiagnosticSink diagnosticSink,
            IntroductionNameProvider introductionNameProvider,
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IServiceProvider serviceProvider,
            CompilationModel compilation ) : base( serviceProvider, diagnosticSink, syntaxGenerationContext, compilation, lexicalScopeProvider )
        {
            this.AspectReferenceSyntaxProvider = aspectReferenceSyntaxProvider;
            this.IntroductionNameProvider = introductionNameProvider;
        }
    }
}