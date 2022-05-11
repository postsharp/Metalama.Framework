// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Advices
{
    internal class FilterMethodTransformation : OverriddenMethodBase
    {
        public FilterMethodTransformation( Advice advice, IMethod overriddenDeclaration ) : base( advice, overriddenDeclaration, ObjectReader.Empty ) { }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var advice = (FilterAdvice) this.Advice;

            var returnValueName = this.OverriddenDeclaration.ReturnType.Is( SpecialType.Void )
                ? null
                : context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" );

            // Execute the templates.
            var success = true;

            success &= advice.TryExecuteTemplates( this.OverriddenDeclaration, context, FilterDirection.Input, out var inputFilterBodies );
            success &= advice.TryExecuteTemplates( this.OverriddenDeclaration, context, FilterDirection.Output, out var outputFilterBodies );

            if ( !success )
            {
                return Enumerable.Empty<IntroducedMember>();
            }

            // Rewrite the method body.
            var proceedExpression = this.CreateProceedExpression( context, TemplateKind.Default ).ToRunTimeExpression().Syntax;

            var statements = new List<StatementSyntax>();
            statements.AddRange( inputFilterBodies );

            if ( outputFilterBodies!.Count > 0 )
            {
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
                    statements.Add( ReturnStatement( IdentifierName( returnValueName ).WithLeadingTrivia( ElasticSpace ) ) );
                }
            }
            else
            {
                if ( returnValueName != null )
                {
                    statements.Add( ReturnStatement( proceedExpression ) );
                }
                else
                {
                    statements.Add( ExpressionStatement( proceedExpression ) );
                }
            }

            return this.GetIntroducedMembersImpl( context, Block( List( statements ) ), false );
        }
    }
}