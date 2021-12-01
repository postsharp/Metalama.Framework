using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Sdk.Simple
{
    internal class Aspect : MethodAspect { }

    [CompilerPlugin]
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