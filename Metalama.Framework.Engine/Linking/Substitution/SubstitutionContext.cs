// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class SubstitutionContext
    {
        private readonly InliningContextIdentifier _inliningContextId;

        public LinkerRewritingDriver RewritingDriver { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public SubstitutionContext( LinkerRewritingDriver rewritingDriver, SyntaxGenerationContext syntaxGenerationContext, InliningContextIdentifier inliningContextId )
        {
            this._inliningContextId = inliningContextId;
            this.RewritingDriver = rewritingDriver;
            this.SyntaxGenerationContext = syntaxGenerationContext;
        }

        internal SubstitutionContext WithInliningContext( InliningContextIdentifier inliningContextId )
        {
            return new SubstitutionContext( this.RewritingDriver, this.SyntaxGenerationContext, inliningContextId );
        }

        public IReadOnlyDictionary<SyntaxNode, SyntaxNodeSubstitution>? GetSubstitutions()
        {
            return this.RewritingDriver.GetSubstitutions( this._inliningContextId );
        }
    }
}