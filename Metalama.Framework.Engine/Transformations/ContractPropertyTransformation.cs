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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class ContractPropertyTransformation : OverridePropertyBaseTransformation
{
    private readonly MethodKind? _targetMethodKind;

    public ContractPropertyTransformation( ContractAdvice advice, IProperty overriddenDeclaration, MethodKind? targetMethodKind ) :
        base( advice, overriddenDeclaration, ObjectReader.Empty ) 
    {
        this._targetMethodKind = targetMethodKind;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var advice = (ContractAdvice) this.ParentAdvice;
        var contextCopy = context;
        BlockSyntax? getterBody, setterBody;

        // Local function that executes the filter for one of the accessors.
        var syntaxGenerationContext = context.SyntaxGenerationContext;

        bool TryExecuteFilters(
            IMethod? accessor,
            ContractDirection direction,
            [NotNullWhen( true )] out List<StatementSyntax>? statements,
            [NotNullWhen( true )] out ExpressionSyntax? proceedExpression,
            out string? returnValueLocalName )
        {
            if ( accessor != null 
                && (this._targetMethodKind == null || this._targetMethodKind == accessor.MethodKind) 
                && advice.Contracts.Any( f => f.AppliesTo( direction ) ) )
            {
                if ( direction == ContractDirection.Output )
                {
                    returnValueLocalName = contextCopy.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" );
                }
                else
                {
                    returnValueLocalName = null;
                }

                if ( !advice.TryExecuteTemplates( this.OverriddenDeclaration, contextCopy, direction, returnValueLocalName, out statements ) )
                {
                    proceedExpression = null;

                    return false;
                }

                var templateKind =
                    accessor.GetIteratorInfo() switch
                    {
                        { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerable or EnumerableKind.UntypedIEnumerable } => TemplateKind.IEnumerable,
                        { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator } => TemplateKind.IEnumerator,
                        _ => TemplateKind.Default,
                    };

                proceedExpression = this.CreateProceedDynamicExpression( contextCopy, accessor, templateKind )
                    .ToExpressionSyntax( syntaxGenerationContext );

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
                ContractDirection.Input,
                out var setterStatements,
                out var setterProceedExpression,
                out _ ) )
        {
            setterStatements.Add( SyntaxFactory.ExpressionStatement( setterProceedExpression ) );
            setterBody = SyntaxFactoryEx.FormattedBlock( setterStatements );
        }
        else
        {
            setterBody = null;
        }

        // Process the getter (output filters).
        if ( TryExecuteFilters(
                this.OverriddenDeclaration.GetMethod,
                ContractDirection.Output,
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

            getterStatements.Add(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( SyntaxFactory.Space ),
                    SyntaxFactory.IdentifierName( getterReturnValueLocalName! ),
                    SyntaxFactory.Token( SyntaxKind.SemicolonToken ) ) );

            getterBody = SyntaxFactoryEx.FormattedBlock( getterStatements );
        }
        else
        {
            getterBody = null;
        }

        // Return if we have no filter at this point. This may be an error condition.
        if ( getterBody == null && setterBody == null )
        {
            return Array.Empty<InjectedMember>();
        }

        if ( this.OverriddenDeclaration.GetMethod != null && getterBody == null )
        {
            getterBody = this.CreateIdentityAccessorBody( context, SyntaxKind.GetAccessorDeclaration );
        }

        if ( this.OverriddenDeclaration.SetMethod != null && setterBody == null )
        {
            setterBody = this.CreateIdentityAccessorBody( context, SyntaxKind.SetAccessorDeclaration );
        }

        return this.GetInjectedMembersImpl( context, getterBody, setterBody );
    }

    public override string ToString() => $"Add default contract to property '{this.TargetDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified )}'";
}