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

internal sealed class ContractIndexerTransformation : OverrideIndexerBaseTransformation
{
    private readonly MethodKind? _targetMethodKind;

    public ContractIndexerTransformation( ContractAdvice advice, IIndexer overriddenIndexer, MethodKind? targetMethodKind ) :
        base( advice, overriddenIndexer, ObjectReader.Empty )
    {
        this._targetMethodKind = targetMethodKind;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var advice = (ContractAdvice) this.ParentAdvice;
        BlockSyntax? getterBody, setterBody;

        // Local function that executes the filter for one of the accessors.
        bool TryExecuteFilters(
            IMethod? accessor,
            out List<StatementSyntax>? inputStatements,
            [NotNullWhen( true )] out ExpressionSyntax? proceedExpression,
            out List<StatementSyntax>? outputStatements,
            out string? returnValueLocalName )
        {
            if ( accessor != null
                 && (this._targetMethodKind == null || this._targetMethodKind == accessor.MethodKind)
                 && advice.Contracts.Any( f => f.AppliesTo( ContractDirection.Input ) ) )
            {
                if ( !advice.TryExecuteTemplates( 
                        this.OverriddenDeclaration,
                        context, 
                        ContractDirection.Input, 
                        null, 
                        accessor.MethodKind == MethodKind.PropertyGet ? RemoveInputContract : null,
                        out inputStatements ) )
                {
                    inputStatements = null;
                    proceedExpression = null;
                    outputStatements = null;
                    returnValueLocalName = null;
                }

                bool RemoveInputContract( Contract contract )
                {
                    var targetDeclaration = contract.TargetDeclaration.GetTarget( this.OverriddenDeclaration.Compilation );

                    return targetDeclaration is not IIndexer;
                }
            }
            else
            {
                inputStatements = null;
            }

            if ( accessor != null
                 && (this._targetMethodKind == null || this._targetMethodKind == accessor.MethodKind)
                 && advice.Contracts.Any( f => f.AppliesTo( ContractDirection.Output ) ) )
            {
                returnValueLocalName = 
                    accessor.MethodKind == MethodKind.PropertyGet 
                        ? context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" ) 
                        : null;

                if ( !advice.TryExecuteTemplates( this.OverriddenDeclaration, context, ContractDirection.Output, returnValueLocalName, null, out outputStatements ) )
                {
                    inputStatements = null;
                    proceedExpression = null;
                    outputStatements = null;

                    return false;
                }
            }
            else
            {
                outputStatements = null;
                returnValueLocalName = null;
            }

            if ( accessor == null
                 || !(this._targetMethodKind == null || this._targetMethodKind == accessor.MethodKind) )
            {
                proceedExpression = null;

                return false;
            }

            proceedExpression = this.CreateProceedDynamicExpression( context, accessor, TemplateKind.Default )
                .ToExpressionSyntax( new( context.Compilation, context.SyntaxGenerationContext ) );

            return true;
        }

        var setterStatements = new List<StatementSyntax>();

        // Process the setter.
        if ( TryExecuteFilters(
                this.OverriddenDeclaration.SetMethod,
                out var setterInputStatements,
                out var setterProceedExpression,
                out var setterOutputStatements,
                out _ ) )
        {
            if ( setterInputStatements != null )
            {
                setterStatements.AddRange( setterInputStatements );
            }

            setterStatements.Add( SyntaxFactory.ExpressionStatement( setterProceedExpression ) );

            if ( setterOutputStatements != null )
            {
                throw new AssertionFailedException( $"Output filter is not possible for indexer setter {this.OverriddenDeclaration}." );
            }

            setterBody = SyntaxFactoryEx.FormattedBlock( setterStatements );
        }
        else
        {
            setterBody = null;
        }

        var getterStatements = new List<StatementSyntax>();

        // Process the getter (output filters).
        if ( TryExecuteFilters(
                this.OverriddenDeclaration.GetMethod,
                out var getterInputStatements,
                out var getterProceedExpression,
                out var getterOutputStatements,
                out var getterReturnValueLocalName ) )
        {
            if ( getterInputStatements != null )
            {
                getterStatements.AddRange( getterInputStatements );
            }

            if ( getterOutputStatements != null )
            {
                getterStatements.Add(
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration( SyntaxFactoryEx.VarIdentifier() )
                            .WithVariables(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier( getterReturnValueLocalName! ).WithTrailingTrivia( SyntaxFactory.ElasticSpace ) )
                                        .WithInitializer( SyntaxFactory.EqualsValueClause( getterProceedExpression ) ) ) ) ) );

                getterStatements.AddRange( getterOutputStatements );

                getterStatements.Add(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( SyntaxFactory.Space ),
                        SyntaxFactory.IdentifierName( getterReturnValueLocalName! ),
                        SyntaxFactory.Token( SyntaxKind.SemicolonToken ) ) );
            }
            else
            {
                getterStatements.Add( SyntaxFactory.ReturnStatement( getterProceedExpression ) );
            }

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