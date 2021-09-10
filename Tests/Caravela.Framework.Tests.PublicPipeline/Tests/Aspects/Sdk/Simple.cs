// @IgnoredDiagnostic(CS1701)
using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Sdk.Simple
{
    class Aspect : Attribute, IAspect<IMethod>
    {
    }

    [CompilerPlugin, AspectWeaver( typeof(Aspect) )]
    class AspectWeaver : IAspectWeaver
    {
        public void Transform( AspectWeaverContext context )
        {
            context.RewriteAspectTargets( new Rewriter() );
        }

        class Rewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                return base.VisitMethodDeclaration( node )!.WithLeadingTrivia( SyntaxFactory.Comment( "// Rewritten." + Environment.NewLine ) );
            }
        }
    }


    // <target>
    class TargetCode
    {
        [Aspect]
        int TransformedMethod( int a ) => 0;
        
        int NotTransformedMethod( int a ) => 0;
    }
}