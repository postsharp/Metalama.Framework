// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal abstract partial class AspectReferenceRenamingSubstitution : SyntaxNodeSubstitution
    {
        public ResolvedAspectReference AspectReference { get; }

        public override SyntaxNode TargetNode => this.AspectReference.RootNode;

        public AspectReferenceRenamingSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base( compilationContext )
        {
            this.AspectReference = aspectReference;
        }

        public sealed override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            if ( this.AspectReference.RootNode != this.AspectReference.SymbolSourceNode )
            {
                // Root node is different that symbol source node - this is introduction in form:
                // <helper_type>.<helper_member>(<symbol_source_node>);
                // We need to get to symbol source node.

                currentNode = this.AspectReference.RootNode switch
                {
                    InvocationExpressionSyntax { ArgumentList: { Arguments.Count: 1 } argumentList } =>
                        argumentList.Arguments[0].Expression,
                    _ => throw new AssertionFailedException( $"{this.AspectReference.RootNode.Kind()} is not in a supported form." )
                };
            }

            switch ( currentNode )
            {
                case MemberAccessExpressionSyntax
                {
                    Expression: IdentifierNameSyntax { Identifier.Text: LinkerInjectionHelperProvider.HelperTypeName },
                    Name.Identifier.Text: LinkerInjectionHelperProvider.FinalizeMemberName
                } finalizerMemberAccess:
                    return this.SubstituteFinalizerMemberAccess( finalizerMemberAccess, substitutionContext );

                case MemberAccessExpressionSyntax
                {
                    Expression: IdentifierNameSyntax { Identifier.Text: LinkerInjectionHelperProvider.HelperTypeName },
                    Name.Identifier.Text: var operatorName
                } operatorMemberAccess when SymbolHelpers.GetOperatorKindFromName( operatorName ) != OperatorKind.None:
                    return this.SubstituteOperatorMemberAccess( operatorMemberAccess, substitutionContext );

                case MemberAccessExpressionSyntax memberAccessExpression:
                    return this.SubstituteMemberAccess( memberAccessExpression, substitutionContext );

                case ConditionalAccessExpressionSyntax conditionalAccessExpression:
                    return this.SubstituteConditionalAccess( conditionalAccessExpression, substitutionContext);

                default:
                    throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
            }
        }

        public abstract string GetTargetMemberName();

        public virtual SyntaxNode? SubstituteFinalizerMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName( this.GetTargetMemberName() ) );
        }

        public virtual SyntaxNode? SubstituteOperatorMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
        {
            var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType ),
                    IdentifierName( this.GetTargetMemberName() ) );
        }

        public abstract SyntaxNode? SubstituteMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext );

        public virtual SyntaxNode? SubstituteConditionalAccess( ConditionalAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
        {
            var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

            if ( this.CompilationContext.SymbolComparer.Is( this.AspectReference.ContainingSemantic.Symbol.ContainingType, targetSymbol.ContainingType ) )
            {
                if ( this.AspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                {
                    throw new AssertionFailedException( Justifications.CoverageMissing );
                }
                else
                {
                    var rewriter = new ConditionalAccessRewriter( this.GetTargetMemberName() );

                    return (ExpressionSyntax) rewriter.Visit( currentNode )!;
                }
            }
            else if ( this.CompilationContext.SymbolComparer.Is( targetSymbol.ContainingType, this.AspectReference.ContainingSemantic.Symbol.ContainingType ) )
            {
                throw new AssertionFailedException( $"Resolved symbol {this.AspectReference.ContainingSemantic.Symbol} is declared in a derived class." );
            }
            else
            {
                var rewriter = new ConditionalAccessRewriter( this.GetTargetMemberName() );

                return (ExpressionSyntax) rewriter.Visit( currentNode )!;
            }
        }

        protected SimpleNameSyntax RewriteName( SimpleNameSyntax name, string targetMemberName )
            => name switch
            {
                GenericNameSyntax genericName => genericName.WithIdentifier( Identifier( targetMemberName.AssertNotNull() ) ),
                IdentifierNameSyntax _ => name.WithIdentifier( Identifier( targetMemberName.AssertNotNull() ) ),
                _ => throw new AssertionFailedException( $"{name.Kind()} is not a supported name." )
            };
    }
}