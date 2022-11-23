﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class MemberInjectionContext : TransformationContext
    {
        public InjectionNameProvider InjectionNameProvider { get; }

        public AspectReferenceSyntaxProvider AspectReferenceSyntaxProvider { get; }

        public MemberInjectionContext(
            UserDiagnosticSink diagnosticSink,
            InjectionNameProvider injectionNameProvider,
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            ITemplateLexicalScopeProvider lexicalScopeProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            CompilationModel compilation ) : base( diagnosticSink, syntaxGenerationContext, compilation, lexicalScopeProvider )
        {
            this.AspectReferenceSyntaxProvider = aspectReferenceSyntaxProvider;
            this.InjectionNameProvider = injectionNameProvider;
        }
    }
}