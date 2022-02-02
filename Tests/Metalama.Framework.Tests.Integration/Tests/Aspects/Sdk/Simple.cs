using System;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Sdk.Simple
{
    internal class Aspect : MethodAspect { }

    [MetalamaPlugIn]
    [AspectWeaver( typeof(Aspect) )]
    internal class AspectWeaver : IAspectWeaver
    {
        public void Transform( AspectWeaverContext context )
        {
            context.RewriteAspectTargets( new Rewriter() );
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                return base.VisitMethodDeclaration( node )!.WithLeadingTrivia( SyntaxFactory.Comment( "// Rewritten." + Environment.NewLine ) );
            }
        }
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private int TransformedMethod( int a ) => 0;

        private int NotTransformedMethod( int a ) => 0;
    }
}