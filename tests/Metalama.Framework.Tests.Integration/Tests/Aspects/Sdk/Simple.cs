using System;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Sdk.Simple
{
    [RequireAspectWeaver( "Metalama.Framework.Tests.PublicPipeline.Aspects.Sdk.Simple.AspectWeaver" )]
    internal class Aspect : MethodAspect { }

    [MetalamaPlugIn]
    internal class AspectWeaver : IAspectWeaver
    {
        public void Transform( AspectWeaverContext context )
        {
            context.RewriteAspectTargets( new Rewriter() );
        }

        private class Rewriter : SafeSyntaxRewriter
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