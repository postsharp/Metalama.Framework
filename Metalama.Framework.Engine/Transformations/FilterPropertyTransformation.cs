// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations;

internal class FilterPropertyTransformation : OverridePropertyBaseTransformation
{
    public FilterPropertyTransformation( FilterAdvice advice, IProperty overriddenDeclaration ) : base( advice, overriddenDeclaration, ObjectReader.Empty ) { }

    public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
    {
        var advice = (FilterAdvice) this.Advice;
        var contextCopy = context;
        BlockSyntax? getterBody, setterBody;

        // Local function that executes the filter for one of the accessors.
        bool TryExecuteFilters(
            IMethod? accessor,
            FilterDirection direction,
            [NotNullWhen( true )] out List<StatementSyntax>? statements,
            [NotNullWhen( true )] out ExpressionSyntax? proceedExpression,
            out string? returnValueLocalName )
        {
            if ( accessor != null && advice.Filters.Any( f => f.AppliesTo( direction ) ) )
            {
                if ( direction == FilterDirection.Output )
                {
                    returnValueLocalName = contextCopy.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" );
                }
                else
                {
                    returnValueLocalName = null;
                }

                if ( !advice.TryExecuteTemplates( this.OverriddenDeclaration, contextCopy!, direction, returnValueLocalName, out statements ) )
                {
                    proceedExpression = null;

                    return false;
                }

                proceedExpression = this.CreateProceedDynamicExpression( contextCopy!, accessor, TemplateKind.Default ).ToRunTimeExpression().Syntax;

                return true;
            }
            else
            {
                statements = null;
                proceedExpression = null;
                returnValueLocalName = null;

                return false;
            }
        }

        // Process the setter (input filters).
        if ( TryExecuteFilters(
                this.OverriddenDeclaration.SetMethod,
                FilterDirection.Input,
                out var setterStatements,
                out var setterProceedExpression,
                out _ ) )
        {
            setterStatements.Add( SyntaxFactory.ExpressionStatement( setterProceedExpression ) );
            setterBody = SyntaxFactory.Block( SyntaxFactory.List( setterStatements ) );
        }
        else
        {
            setterBody = null;
        }

        // Process the getter (output filters).
        if ( TryExecuteFilters(
                this.OverriddenDeclaration.GetMethod,
                FilterDirection.Output,
                out var getterStatements,
                out var getterProceedExpression,
                out var getterReturnValueLocalName ) )
        {
            getterStatements.Insert(
                0,
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName(
                                SyntaxFactory.Identifier(
                                    SyntaxFactory.TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    SyntaxFactory.TriviaList( SyntaxFactory.ElasticSpace ) ) ) )
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier( getterReturnValueLocalName! ).WithTrailingTrivia( SyntaxFactory.ElasticSpace ) )
                                    .WithInitializer( SyntaxFactory.EqualsValueClause( getterProceedExpression ) ) ) ) ) );

            getterStatements.Add( SyntaxFactory.ReturnStatement( SyntaxFactory.IdentifierName( getterReturnValueLocalName! ) ) );
            getterBody = SyntaxFactory.Block( SyntaxFactory.List( getterStatements ) );
        }
        else
        {
            getterBody = null;
        }

        // Return if we have no filter at this point. This may be an error condition.
        if ( getterBody == null && setterBody == null )
        {
            return Array.Empty<IntroducedMember>();
        }

        if ( this.OverriddenDeclaration.GetMethod != null && getterBody == null )
        {
            getterBody = this.CreateIdentityAccessorBody( SyntaxKind.GetAccessorDeclaration, context.SyntaxGenerationContext );
        }

        if ( this.OverriddenDeclaration.SetMethod != null && setterBody == null )
        {
            setterBody = this.CreateIdentityAccessorBody( SyntaxKind.SetAccessorDeclaration, context.SyntaxGenerationContext );
        }

        return this.GetIntroducedMembersImpl( context, getterBody, setterBody );
    }
}