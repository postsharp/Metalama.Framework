// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class ContractMethodTransformation : OverrideMethodBaseTransformation
    {
        public ContractMethodTransformation( ContractAdvice advice, IMethod overriddenDeclaration ) :
            base( advice, overriddenDeclaration, ObjectReader.Empty ) { }

        public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
        {
            var advice = (ContractAdvice) this.ParentAdvice;

            // Execute the templates.

            _ = advice.TryExecuteTemplates( this.OverriddenDeclaration, context, ContractDirection.Input, null, null, out var inputFilterBodies );

            List<StatementSyntax>? outputFilterBodies;
            string? returnValueName;

            if ( advice.Contracts.Any( f => f.AppliesTo( ContractDirection.Output ) ) )
            {
                returnValueName = this.OverriddenDeclaration.ReturnType.Is( SpecialType.Void )
                    ? null
                    : context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" );

                _ = !advice.TryExecuteTemplates(
                    this.OverriddenDeclaration,
                    context,
                    ContractDirection.Output,
                    returnValueName,
                    null,
                    out outputFilterBodies );
            }
            else
            {
                outputFilterBodies = null;
                returnValueName = null;
            }

            var iteratorInfo = this.OverriddenDeclaration.GetIteratorInfo();

            // Determine the kind of template the transformation will simulate.
            var templateKind = iteratorInfo switch
            {
                { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerable or EnumerableKind.UntypedIEnumerable } => TemplateKind.IEnumerable,
                { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator } => TemplateKind.IEnumerator,
                { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerable } => TemplateKind.IAsyncEnumerable,
                { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerator } => TemplateKind.IAsyncEnumerator,
                _ => TemplateKind.Default,
            };

            // Rewrite the method body.
            var proceedExpression = this.CreateProceedExpression( context, templateKind ).ToExpressionSyntax( context.SyntaxGenerationContext );

            var statements = new List<StatementSyntax>();

            if ( inputFilterBodies != null )
            {
                statements.AddRange( inputFilterBodies );
            }

            if ( outputFilterBodies is { Count: > 0 } )
            {
                if ( this.OverriddenDeclaration.IsAsync && this.OverriddenDeclaration.GetAsyncInfo().ResultType.Is( SpecialType.Void ) )
                {
                    throw new AssertionFailedException( $"{this.OverriddenDeclaration} is async, does not return anything, but has output filters." );
                }

                if ( templateKind != TemplateKind.Default )
                {
                    throw new NotImplementedException( "Contracts on return values of iterators are not yet supported." );
                }

                if ( returnValueName != null )
                {
                    statements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName(
                                        Identifier(
                                            TriviaList(),
                                            SyntaxKind.VarKeyword,
                                            "var",
                                            "var",
                                            TriviaList( ElasticSpace ) ) ) )
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator( Identifier( returnValueName ).WithTrailingTrivia( ElasticSpace ) )
                                            .WithInitializer( EqualsValueClause( proceedExpression ) ) ) ) ) );
                }
                else
                {
                    statements.Add( ExpressionStatement( proceedExpression ) );
                }

                statements.AddRange( outputFilterBodies );

                if ( returnValueName != null )
                {
                    statements.Add(
                        ReturnStatement(
                            Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                            IdentifierName( returnValueName ),
                            Token( SyntaxKind.SemicolonToken ) ) );
                }
            }
            else
            {
                if ( templateKind is TemplateKind.IEnumerable or TemplateKind.IAsyncEnumerable )
                {
                    var returnItemName = context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnItem" );

                    statements.Add(
                        ForEachStatement(
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.IsAsync
                                ? Token( TriviaList(), SyntaxKind.AwaitKeyword, TriviaList( ElasticSpace ) )
                                : default,
                            Token( SyntaxKind.ForEachKeyword ),
                            Token( SyntaxKind.OpenParenToken ),
                            IdentifierName( Identifier( TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList( ElasticSpace ) ) ),
                            Identifier( returnItemName ),
                            Token( TriviaList( ElasticSpace ), SyntaxKind.InKeyword, TriviaList( ElasticSpace ) ),
                            proceedExpression,
                            Token( SyntaxKind.CloseParenToken ),
                            Block(
                                YieldStatement(
                                    SyntaxKind.YieldReturnStatement,
                                    Token( TriviaList(), SyntaxKind.YieldKeyword, TriviaList( ElasticSpace ) ),
                                    Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                    IdentifierName( returnItemName ),
                                    Token( SyntaxKind.SemicolonToken ) ) ) ) );
                }
                else if ( templateKind is TemplateKind.IEnumerator or TemplateKind.IAsyncEnumerator )
                {
                    var enumeratorName = context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnEnumerator" );

                    statements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName( Identifier( TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList( ElasticSpace ) ) ),
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier( enumeratorName ),
                                        null,
                                        EqualsValueClause( proceedExpression ) ) ) ) ) );

                    ExpressionSyntax moveNextExpression =
                        this.OverriddenDeclaration.IsAsync
                            ? AwaitExpression(
                                Token( TriviaList(), SyntaxKind.AwaitKeyword, TriviaList( ElasticSpace ) ),
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName( enumeratorName ),
                                        IdentifierName( "MoveNextAsync" ) ) ) )
                            : InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName( enumeratorName ),
                                    IdentifierName( "MoveNext" ) ) );

                    statements.Add(
                        WhileStatement(
                            List<AttributeListSyntax>(),
                            Token( TriviaList(), SyntaxKind.WhileKeyword, TriviaList() ),
                            Token( TriviaList(), SyntaxKind.OpenParenToken, TriviaList() ),
                            moveNextExpression,
                            Token( TriviaList(), SyntaxKind.CloseParenToken, TriviaList() ),
                            Block(
                                YieldStatement(
                                    SyntaxKind.YieldReturnStatement,
                                    Token( TriviaList(), SyntaxKind.YieldKeyword, TriviaList( ElasticSpace ) ),
                                    Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName( enumeratorName ),
                                        IdentifierName( "Current" ) ),
                                    Token( SyntaxKind.SemicolonToken ) ) ) ) );
                }
                else if ( this.OverriddenDeclaration.ReturnType.Is( SpecialType.Void )
                          || (this.OverriddenDeclaration.IsAsync && this.OverriddenDeclaration.GetAsyncInfo().ResultType.Is( SpecialType.Void )) )
                {
                    statements.Add( ExpressionStatement( proceedExpression ) );
                }
                else
                {
                    statements.Add( ReturnStatement( proceedExpression ) );
                }
            }

            return this.GetInjectedMembersImpl( context, SyntaxFactoryEx.FormattedBlock( statements ), this.OverriddenDeclaration.IsAsync );
        }

        public override FormattableString ToDisplayString() => $"Add default contract to method '{this.TargetDeclaration}'";
    }
}