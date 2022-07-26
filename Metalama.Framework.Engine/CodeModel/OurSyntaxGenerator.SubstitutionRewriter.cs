// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class OurSyntaxGenerator
{
    private class SubstitutionRewriter : SafeSyntaxRewriter
    {
        private readonly IReadOnlyDictionary<string, TypeSyntax> _substitutions;

        public SubstitutionRewriter( IReadOnlyDictionary<string, TypeSyntax> substitutions )
        {
            this._substitutions = substitutions;
        }

        public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
        {
            if ( this._substitutions.TryGetValue( node.Identifier.Text, out var substitution ) )
            {
                return substitution;
            }
            else
            {
                return base.VisitIdentifierName( node );
            }
        }
    }
}