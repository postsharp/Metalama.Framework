﻿using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Sdk
{
    [RequireAspectWeaver( "Metalama.Framework.Tests.Integration.Tests.Licensing.Sdk.AspectWeaver" )]
    internal class Aspect : MethodAspect { }

    [MetalamaPlugIn]
    internal class AspectWeaver : IAspectWeaver
    {
        public Task TransformAsync( AspectWeaverContext context )
        {
            return context.RewriteAspectTargetsAsync( new Rewriter() );
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