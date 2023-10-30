// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes.Implementations
{
    internal sealed partial class RemoveAttributeCodeAction
    {
        private sealed class RemoveAttributeRewriter : SafeSyntaxRewriter
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
                var attributes = node.Attributes.SelectAsImmutableArray( this.VisitAttribute ).WhereNotNull().Cast<AttributeSyntax>().ToReadOnlyList();

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