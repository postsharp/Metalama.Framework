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
            private readonly IReadOnlyList<AspectPart> _orderedAspectParts;
            private readonly ImmutableMultiValueDictionary<MemberDeclarationSyntax, IntroducedMember> _introducedMemberLookup;
            public OverrideOrderRewriter( CSharpCompilation compilation, IReadOnlyList<AspectPart> orderedAspectParts, IReadOnlyList<ISyntaxTreeIntroduction> allTransformations )
            {
                this._compilation = compilation;
                this._orderedAspectParts = orderedAspectParts;
                this._introducedMemberLookup =
                    ImmutableMultiValueDictionary<MemberDeclarationSyntax, IntroducedMember>.Empty;
                //allTransformations
                //.OfType<IOverriddenElement>()
                //.OfType<IMemberIntroduction>()
                //.SelectMany( x => x.GetIntroducedMembers().Select( i => (OverriddenElement: ((IOverriddenElement) x).OverriddenElement, IntroducedMember: i) ) )
                //.ToMultiValueDictionary( x => x.OverriddenElement, x => x.IntroducedMember );
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
                    this._orderedAspectParts
                    .Select( ( x, i ) => (Index: i, Value: x) )
                    .Single( x => x.Value.AspectType.Name == annotation.AspectTypeName && x.Value.PartName == annotation.PartName )
                    .Index;

                // The callee is the original/introduced method.
                var callee = (IMethodSymbol?) this._compilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node ).Symbol;

                var declarationSyntax = (MethodDeclarationSyntax)callee.DeclaringSyntaxReferences.Single().GetSyntax();

                // Now look for IntroducedMembers targeting this syntax.
                var overrides = this._introducedMemberLookup[declarationSyntax];

                var precedingOverrides =
                    this._orderedAspectParts
                    .Select( ( x, i ) => (Index: i, Value: overrides.SingleOrDefault( o => o.AspectPart == x.ToAspectPartId() )) )
                    .Where( x => x.Value != null && x.Index < currentMethodPosition );

                if ( !precedingOverrides.Any() )
                {
                    // There is not preceding override, so we leave the existing method call to the original/introduced declaration.
                    return base.VisitInvocationExpression( node );
                }

                var overrideImmediatelyBefore = precedingOverrides.Last().Value;

                // TODO: not correct assumption?
                var memberAccess = (MemberAccessExpressionSyntax) node.Expression;

                return
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            (ExpressionSyntax)this.Visit( memberAccess.Expression ),
                            IdentifierName( ((MethodDeclarationSyntax) overrideImmediatelyBefore.Syntax).Identifier )
                            ),
                        (ArgumentListSyntax)this.VisitArgumentList(node.ArgumentList)
                    );
            }
        }
    }
}
