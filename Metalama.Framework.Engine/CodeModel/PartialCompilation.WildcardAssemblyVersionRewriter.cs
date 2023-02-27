// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    public abstract partial class PartialCompilation
    {
        private sealed class WildcardAssemblyVersionRewriter : CSharpSyntaxRewriter
        {
            private readonly LiteralExpressionSyntax _targetSyntax;
            private readonly Version _assemblyVersion;

            public WildcardAssemblyVersionRewriter( LiteralExpressionSyntax targetSyntax, Version assemblyVersion )
            {
                this._targetSyntax = targetSyntax;
                this._assemblyVersion = assemblyVersion;
            }

            public override SyntaxNode? VisitLiteralExpression( LiteralExpressionSyntax node )
            {
                if ( node == this._targetSyntax )
                {
                    return
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal( this._assemblyVersion.ToString() ) );
                }
                else
                {
                    return base.VisitLiteralExpression( node );
                }
            }

            public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
            {
                // Do not visit the entire tree.
                return node.WithAttributeLists( this.VisitList( node.AttributeLists ) );
            }
        }
    }
}