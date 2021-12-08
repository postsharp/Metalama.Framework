// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Impl.DesignTime.CodeFixes
{
    internal partial class CodeFixBuilder
    {
        private class RemoveAttributeRewriter : CSharpSyntaxRewriter
        {
            private readonly SemanticModel _semanticModel;
            private readonly ITypeSymbol _attributeType;

            public RemoveAttributeRewriter( SemanticModel semanticModel, ITypeSymbol attributeType )
            {
                this._semanticModel = semanticModel;
                this._attributeType = attributeType;
            }

            public override SyntaxNode? VisitAttribute( AttributeSyntax node )
            {
                var thisAttributeType = (IMethodSymbol?) this._semanticModel.GetSymbolInfo( node.Name ).Symbol;

                if ( thisAttributeType != null && SymbolEqualityComparer.Default.Equals( thisAttributeType.ContainingType, this._attributeType ) )
                {
                    return null;
                }
                else
                {
                    return base.VisitAttribute( node );
                }
            }

            public override SyntaxNode? VisitAttributeList( AttributeListSyntax node )
            {
                var attributes = node.Attributes.Select( this.VisitAttribute ).WhereNotNull().ToList();

                if ( attributes.Count == 0 )
                {
                    return null;
                }
                else
                {
                    // We're loosing the trivia on the commas while doing this.
                    return node.WithAttributes( SyntaxFactory.SeparatedList( attributes ) );
                }
            }
        }
    }
}