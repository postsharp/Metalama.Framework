// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        /// <summary>
        /// Transforms an original body using substitutions.
        /// </summary>
        internal class SubstitutingRewriter : SafeSyntaxRewriter
        {
            private readonly SubstitutionContext _substitutionContext;

            public SubstitutingRewriter( SubstitutionContext substitutionContext )
            {
                this._substitutionContext = substitutionContext;
            }

            protected override SyntaxNode? VisitCore( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                var substitutions = this._substitutionContext.GetSubstitutions();

                if ( substitutions != null && substitutions.TryGetValue( node, out var substitution ) )
                {
                    var currentNode = base.VisitCore( node ).AssertNotNull();
                    return substitution.Substitute( currentNode, this._substitutionContext );
                }
                else
                {
                    return base.VisitCore( node );
                }
            }
        }
    }
}