// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Templating;

internal static partial class SyntaxAnnotationExtensions
{
    private class HasAnyCompileTimeOnlyCodeVisitor : SafeSyntaxVisitor<bool>
    {
        public static HasAnyCompileTimeOnlyCodeVisitor Instance { get; } = new();
        
        private HasAnyCompileTimeOnlyCodeVisitor() { }

        public override bool DefaultVisit( SyntaxNode node )
        {
            if ( node.GetScopeFromAnnotation().GetValueOrDefault().GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
            {
                return true;
            }
            else
            {
                foreach ( var child in node.ChildNodesAndTokens() )
                {
                    if ( child.IsNode && this.Visit( child.AsNode() ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}