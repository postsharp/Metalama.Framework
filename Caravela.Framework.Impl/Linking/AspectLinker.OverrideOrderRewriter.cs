using Caravela.Framework.Impl.AspectOrdering;
using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class AspectLinker
    {
        public class OverrideOrderRewriter : CSharpSyntaxRewriter
        {
            private readonly CSharpCompilation _compilation;
            private readonly IReadOnlyList<OrderedAspectLayer> _orderedAspectLayers;
            private readonly ImmutableMultiValueDictionary<ISymbol, IntroducedMember> _overrideLookup;

            public OverrideOrderRewriter( 
                CSharpCompilation compilation, 
                IReadOnlyList<OrderedAspectLayer> orderedAspectLayers, 
                ImmutableMultiValueDictionary<ISymbol, IntroducedMember> overrideLookup )
            {
                this._compilation = compilation;
                this._orderedAspectLayers = orderedAspectLayers;
                this._overrideLookup = overrideLookup;
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var newMembers = new List<MemberDeclarationSyntax>();

                foreach (var member in node.Members)
                {
                    if (member is not MethodDeclarationSyntax methodDeclaration)
                    {
                        newMembers.Add( (MemberDeclarationSyntax) this.Visit( member ) );
                        continue;
                    }    

                    var originalSymbol = (IMethodSymbol) this._compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( member );

                    var overrides = this._overrideLookup[originalSymbol];

                    if (!overrides.IsEmpty)
                    {
                        var lastOverride =
                            this._orderedAspectLayers
                            .Select( ( x, i ) => (Index: i, Value: overrides.SingleOrDefault( o => o.AspectLayerId.Equals( x ) )) )
                            .Where(x => x.Value != null)
                            .Last().Value;

                        // This is method override - we need to move the body into another method called __{MethodName}__OriginalMethod.
                        var originalMethodDeclaration = (MethodDeclarationSyntax) member;
                        var originalBodyMethodName = GetOriginalBodyMethodName( originalMethodDeclaration.Identifier.ValueText );
                        var lastOverrideName = ((MethodDeclarationSyntax) lastOverride.Syntax).Identifier;

                        var invocation = InvocationExpression(
                            !originalSymbol.IsStatic
                            ? MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName( lastOverrideName )
                                )
                            : IdentifierName( lastOverrideName ),
                            ArgumentList(
                                SeparatedList(
                                    originalSymbol.Parameters.Select( x => Argument( IdentifierName( x.Name! ) ) )
                                    )
                                )
                            );

                        newMembers.Add( originalMethodDeclaration.WithBody(
                            Block(
                                originalSymbol.ReturnsVoid
                                ? ExpressionStatement( invocation )
                                : ReturnStatement( invocation)
                            ) ) );

                        newMembers.Add( originalMethodDeclaration.WithIdentifier( Identifier( originalBodyMethodName ) ) );
                    }
                    else
                    {
                        newMembers.Add( (MemberDeclarationSyntax) this.Visit( member ) );
                    }
                }

                return node.WithMembers( List( newMembers ) );
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                var annotation = node.GetLinkerAnnotation();

                if ( annotation == null )
                {
                    return base.VisitInvocationExpression( node );
                }

                // TODO: optimize.
                var currentMethodPosition =
                    this._orderedAspectLayers
                    .Select( ( x, i ) => (Index: i, Value: x) )
                    .Single( x => x.Value.AspectLayerId == annotation.AspectLayerId )
                    .Index;

                // The callee is the original/introduced method.
                var callee = (IMethodSymbol?) this._compilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node ).Symbol;

                var declarationSyntax = (MethodDeclarationSyntax) callee.DeclaringSyntaxReferences.Single().GetSyntax();

                var declarationSymbol = this._compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( declarationSyntax );

                // Now look for IntroducedMembers targeting this syntax.
                var overrides = this._overrideLookup[declarationSymbol];

                var precedingOverrides =
                    this._orderedAspectLayers
                    .Select( ( x, i ) => (Index: i, Value: overrides.SingleOrDefault( o => o.AspectLayerId.Equals( x ) )) )
                    .Where( x => x.Value != null && x.Index < currentMethodPosition );

                // TODO: simplify
                if ( node.Expression is MemberAccessExpressionSyntax memberAccess )
                {
                    if ( !precedingOverrides.Any() )
                    {
                        // There is no preceding override, so we call the moved method body.
                        return
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    (ExpressionSyntax) this.Visit( memberAccess.Expression ),
                                    IdentifierName( GetOriginalBodyMethodName( declarationSyntax.Identifier.ValueText ) )
                                    ),
                                (ArgumentListSyntax) this.VisitArgumentList( node.ArgumentList )
                            );
                    }
                    else
                    {
                        var overrideImmediatelyBefore = precedingOverrides.Last().Value;

                        return
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    (ExpressionSyntax) this.Visit( memberAccess.Expression ),
                                    IdentifierName( ((MethodDeclarationSyntax) overrideImmediatelyBefore.Syntax).Identifier )
                                    ),
                                (ArgumentListSyntax) this.VisitArgumentList( node.ArgumentList )
                            );
                    }
                }
                else if ( node.Expression is IdentifierNameSyntax identifier )
                {
                    if ( !precedingOverrides.Any() )
                    {
                        // There is no preceding override, so we call the moved method body.
                        return
                            InvocationExpression(
                                IdentifierName( GetOriginalBodyMethodName( declarationSyntax.Identifier.ValueText ) ),
                                (ArgumentListSyntax) this.VisitArgumentList( node.ArgumentList )
                            );
                    }
                    else
                    {
                        var overrideImmediatelyBefore = precedingOverrides.Last().Value;

                        return
                            InvocationExpression(
                                IdentifierName( ((MethodDeclarationSyntax) overrideImmediatelyBefore.Syntax).Identifier ),
                                (ArgumentListSyntax) this.VisitArgumentList( node.ArgumentList )
                            );
                    }
                }
                else
                    throw new NotImplementedException();
            }

            private static string GetOriginalBodyMethodName( string methodName )
                => $"__{methodName}__OriginalBody";
        }
    }
}
