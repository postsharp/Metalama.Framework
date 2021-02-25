using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        private class LinkingRewriter : CSharpSyntaxRewriter
        {
            private readonly CSharpCompilation _intermediateCompilation;
            private readonly LinkerTransformationRegistry _transformationRegistry;
            private readonly LinkerReferenceRegistry _referenceRegistry;
            private readonly IReadOnlyList<AspectPart> _orderedAspectParts;

            public LinkingRewriter(
                IReadOnlyList<AspectPart> orderedAspectParts,
                LinkerTransformationRegistry transformationRegistry,
                CSharpCompilation intermediateCompilation,
                LinkerReferenceRegistry referenceRegistry )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._orderedAspectParts = orderedAspectParts;
                this._transformationRegistry = transformationRegistry;
                this._referenceRegistry = referenceRegistry;
            }

            private static string GetOriginalBodyMethodName( string methodName )
                => $"__{methodName}__OriginalBody";

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

                    if ( this._referenceRegistry.IsOverrideMethod( symbol ) )
                    {
                        // Override method.
                        if ( this._referenceRegistry.IsBodyInlineable( symbol ) )
                        {
                            // Method's body is inlineable, the method itself can be removed.
                            continue;
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
                        var transformedMethod = ((MethodDeclarationSyntax) member).WithBody( this.GetRewrittenMethodBody( semanticModel, method, symbol ) );
                        newMembers.Add( transformedMethod );

                        if ( !this._referenceRegistry.IsBodyInlineable( symbol ) )
                        {
                            // TODO: This should be inserted after all other overrides.
                            // This is target method that is not inlineable, we need to a separate declaration.
                            newMembers.Add( this.GetOriginalBodyMethod( method ) );
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

            private BlockSyntax? GetTrampolineBody( MethodDeclarationSyntax method, IMethodSymbol targetSymbol )
            {
                var invocation =
                    InvocationExpression(
                        GetInvocationTarget(),
                        ArgumentList( SeparatedList( method.ParameterList.Parameters.Select( x => Argument( IdentifierName( x.Identifier ) ) ) ) ) );

                if ( targetSymbol.ReturnsVoid )
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
