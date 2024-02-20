﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

internal sealed class ContractPropertyTransformation : OverridePropertyBaseTransformation, IInsertStatementTransformation
{
    private readonly MethodKind? _targetMethodKind;

    public ContractPropertyTransformation( ContractAdvice advice, IProperty overriddenDeclaration, MethodKind? targetMethodKind ) :
        base( advice, overriddenDeclaration, ObjectReader.Empty ) 
    {
        this._targetMethodKind = targetMethodKind;
    }

    public IMember TargetMember => this.OverriddenDeclaration;

    public IEnumerable<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        if (this.OverriddenDeclaration.SetMethod == null)
        {
            return Array.Empty<InsertedStatement>();
        }

        var advice = (ContractAdvice) this.ParentAdvice;

        if ( !advice.TryExecuteTemplates( this.OverriddenDeclaration, context, ContractDirection.Input, null, null, out var inputFilterBodies )
            || inputFilterBodies.Count == 0 )
        {
            return Array.Empty<InsertedStatement>();
        }

        return inputFilterBodies.SelectAsArray( b => new InsertedStatement( b, this.OverriddenDeclaration.SetMethod, this, InsertedStatementKind.CurrentEntry ) );
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var advice = (ContractAdvice) this.ParentAdvice;
        BlockSyntax? getterBody;

        // Local function that executes the filter for one of the accessors.
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
                    returnValueLocalName = context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" );
                }
                else
                {
                    returnValueLocalName = null;
                }

                if ( !advice.TryExecuteTemplates( this.OverriddenDeclaration, context, direction, returnValueLocalName, null, out statements ) || statements.Count == 0 )
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

                proceedExpression = this.CreateProceedDynamicExpression( context, accessor, templateKind )
                    .ToExpressionSyntax( new( context.Compilation, context.SyntaxGenerationContext ) );

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
                    SyntaxFactory.VariableDeclaration( SyntaxFactoryEx.VarIdentifier() )
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier( default, getterReturnValueLocalName!, new( SyntaxFactory.ElasticSpace ) ) )
                                    .WithInitializer( SyntaxFactory.EqualsValueClause( getterProceedExpression ) ) ) ) ) );

            getterStatements.Add(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                    SyntaxFactory.IdentifierName( getterReturnValueLocalName! ),
                    SyntaxFactory.Token( SyntaxKind.SemicolonToken ) ) );

            getterBody = SyntaxFactoryEx.FormattedBlock( getterStatements );
        }
        else
        {
            getterBody = null;
        }

        // We need to force an empty override for auto properties with input contracts.
        // This is caused by a current design of the linker - auto-property deconstruction happens during the final rewriting and
        // insert statement transformation requires a body to insert statements into.
        var isAutoProperty = ((IProperty) this.OverriddenDeclaration).IsAutoPropertyOrField.AssertNotNull();

        // The same goes for record properties
        var isRecordProperty = this.OverriddenDeclaration.ContainingDeclaration is INamedType { TypeKind: TypeKind.RecordClass or TypeKind.RecordStruct };

        var requiresEmptyOverride =
            advice.Contracts.Any( c => c.AppliesTo( ContractDirection.Input ) )
            && (isAutoProperty || isRecordProperty);

        // Return if we have no filter at this point. This may be an error condition.
        if ( getterBody == null && !requiresEmptyOverride )
        {
            return Array.Empty<InjectedMember>();
        }

        if ( this.OverriddenDeclaration.GetMethod != null && getterBody == null )
        {
            getterBody = this.CreateIdentityAccessorBody( context, SyntaxKind.GetAccessorDeclaration );
        }

        return
            this.GetInjectedMembersImpl(
                context,
                getterBody,
                this.OverriddenDeclaration.SetMethod != null
                ? this.CreateIdentityAccessorBody( context, SyntaxKind.SetAccessorDeclaration )
                : null );
    }

    public override string ToString() => $"Add default contract to property '{this.TargetDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified )}'";
}