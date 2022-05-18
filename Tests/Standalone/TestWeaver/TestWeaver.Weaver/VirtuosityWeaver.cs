// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Metalama.Open.Virtuosity
{
    [MetalamaPlugIn]
    public class VirtuosityWeaver : IAspectWeaver
    {
        void IAspectWeaver.Transform( AspectWeaverContext context )
        {
            context.RewriteAspectTargets( new Rewriter() );
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private static readonly SyntaxKind[]? _forbiddenModifiers = new[] { StaticKeyword, VirtualKeyword, OverrideKeyword };
            private static readonly SyntaxKind[]? _requiredModifiers = new[] { PublicKeyword, ProtectedKeyword, InternalKeyword };

            public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( node.Parent is not ClassDeclarationSyntax classDeclaration )
                {
                    return node;
                }

                // Note: this won't work if the method is in one part of a partial class and only the other part has the sealed modifier
                if ( classDeclaration.Modifiers.Any( SealedKeyword ) )
                {
                    return node;
                }

                var modifiers = node.Modifiers;

                // Remove the sealed modifier.
                var sealedToken = node.Modifiers.FirstOrDefault( modifier => modifier.IsKind( SyntaxKind.SealedKeyword ) );

                if ( !sealedToken.IsKind( SyntaxKind.None ) )
                {
                    node = node.WithModifiers( node.Modifiers.Remove( sealedToken ) );
                }

                // Add the virtual modifier.
                if ( !_forbiddenModifiers.Any( modifier => node.Modifiers.Any( modifier ) )
                    && _requiredModifiers.Any( modifier => node.Modifiers.Any( modifier ) ) )
                {
                    node = node.AddModifiers( SyntaxFactory.Token( VirtualKeyword ).WithTrailingTrivia( SyntaxFactory.ElasticSpace ) );
                }


                return node;
            }
        }
    }
}
