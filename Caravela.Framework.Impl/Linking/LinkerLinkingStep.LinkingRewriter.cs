// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Rewriter which rewrites classes and methods producing the linked and inlined syntax tree.
        /// </summary>
        private class LinkingRewriter : CSharpSyntaxRewriter
        {
            private readonly CSharpCompilation _intermediateCompilation;
            private readonly LinkerAnalysisRegistry _referenceRegistry;

            public LinkingRewriter(
                CSharpCompilation intermediateCompilation,
                LinkerAnalysisRegistry referenceRegistry )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._referenceRegistry = referenceRegistry;
            }

            internal static string GetOriginalBodyMethodName( string methodName ) => $"__{methodName}__OriginalBody";

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                // TODO: Other transformations than method overrides.
                var newMembers = new List<MemberDeclarationSyntax>();

                foreach ( var member in node.Members )
                {
                    if ( member is not MethodDeclarationSyntax )
                    {
                        newMembers.Add( (MemberDeclarationSyntax) this.Visit( member ) );

                        continue;
                    }

                    var method = (MethodDeclarationSyntax) member;
                    var semanticModel = this._intermediateCompilation.GetSemanticModel( node.SyntaxTree );
                    var symbol = semanticModel.GetDeclaredSymbol( method )!;

                    if ( this._referenceRegistry.IsOverride( symbol ) )
                    {
                        // Override method.
                        if ( this._referenceRegistry.IsBodyInlineable( symbol ) )
                        {
                            // Method's body is inlineable, the method itself can be removed.
                        }
                        else
                        {
                            // Rewrite the method.
                            var transformedMethod = ((MethodDeclarationSyntax) member).WithBody( this.GetRewrittenMethodBody( semanticModel, method, symbol ) );
                            newMembers.Add( transformedMethod );
                        }
                    }
                    else if ( this._referenceRegistry.IsOverrideTarget( symbol ) )
                    {
                        // Override target, i.e. original method or introduced method stub.
                        var lastOverrideSymbol = (IMethodSymbol) this._referenceRegistry.GetLastOverride( symbol );

                        if ( !this._referenceRegistry.IsBodyInlineable( lastOverrideSymbol ) )
                        {
                            // Body of the last (outermost) override is not inlineable. We need to emit a trampoline method.
                            var transformedMethod = method.WithBody( this.GetTrampolineMethodBody( method, lastOverrideSymbol ) )
                                                          .WithLeadingTrivia( method.GetLeadingTrivia() )
                                                          .WithTrailingTrivia( method.GetTrailingTrivia() );

                            newMembers.Add( transformedMethod );
                        }
                        else
                        {
                            // Body of the last (outermost) override is inlineable. We will run inlining on the override's body and place replace the current body with the result.
                            var lastOverrideSyntax = (MethodDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                            // Inline overrides into this method.
                            var transformedMethod = ((MethodDeclarationSyntax) member).WithBody(
                                this.GetRewrittenMethodBody( semanticModel, lastOverrideSyntax, lastOverrideSymbol ) );

                            newMembers.Add( transformedMethod );
                        }

                        if ( !this._referenceRegistry.IsBodyInlineable( symbol ) )
                        {
                            // TODO: This should be inserted after all other overrides.
                            // This is target method that is not inlineable, we need to a separate declaration.
                            var originalBodyMethod = this.GetOriginalBodyMethod( method );
                            newMembers.Add( originalBodyMethod );
                        }
                    }
                    else
                    {
                        // Normal method without any transformations.
                        newMembers.Add( method );
                    }
                }

                return node.WithMembers( List( newMembers ) );
            }

            private BlockSyntax? GetTrampolineMethodBody( MethodDeclarationSyntax method, IMethodSymbol targetSymbol )
            {
                // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.
                var invocation =
                    InvocationExpression(
                        GetInvocationTarget(),
                        ArgumentList( SeparatedList( method.ParameterList.Parameters.Select( x => Argument( IdentifierName( x.Identifier ) ) ) ) ) );

                if ( !targetSymbol.ReturnsVoid )
                {
                    return Block( ReturnStatement( invocation ) );
                }
                else
                {
                    return Block( ExpressionStatement( invocation ) );
                }

                ExpressionSyntax GetInvocationTarget()
                {
                    if ( targetSymbol.IsStatic )
                    {
                        return IdentifierName( targetSymbol.Name );
                    }
                    else
                    {
                        return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) );
                    }
                }
            }

            private BlockSyntax? GetRewrittenMethodBody( SemanticModel semanticModel, MethodDeclarationSyntax method, IMethodSymbol symbol )
            {
                // Create inlining rewriter and inline calls into this method's body.
                var inliningRewriter = new InliningRewriter( this._referenceRegistry, semanticModel, symbol );

                return (BlockSyntax) inliningRewriter.VisitBlock( method.Body.AssertNotNull() ).AssertNotNull();
            }

            private MemberDeclarationSyntax GetOriginalBodyMethod( MethodDeclarationSyntax method )
            {
                return method.WithIdentifier( Identifier( GetOriginalBodyMethodName( method.Identifier.ValueText ) ) );
            }
        }
    }
}