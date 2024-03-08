// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class ContractMethodTransformation : ContractBaseTransformation
{
    public new IMethod TargetMember => (IMethod)base.TargetMember;

    public ContractMethodTransformation( 
        Advice advice,
        IMethod targetMethod,
        IParameter contractTarget,
        ContractDirection contractDirection,
        TemplateMember<IMethod> template,
        IObjectReader templateArguments,
        IObjectReader tags ) : base( advice, targetMethod, contractTarget, contractDirection, template, templateArguments, tags ) 
    {
    }

    public override IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        switch ( this.ContractTarget )
        {
            case IParameter { IsReturnParameter: true } returnValueParam:
                {
                    Invariant.Assert( this.ContractDirection == ContractDirection.Output );

                    var variableName = context.GetReturnValueVariableName();

                    if ( !this.TryExecuteTemplate( context, IdentifierName( variableName ), out var contractBlock ) )
                    {
                        return Array.Empty<InsertedStatement>();
                    }
                    else
                    {
                        return new[] { new InsertedStatement( contractBlock, returnValueParam, this, InsertedStatementKind.OutputContract ) };
                    }
                }

            case IParameter param:
                {
                    Invariant.Assert( this.ContractDirection is ContractDirection.Output or ContractDirection.Input or ContractDirection.Both );

                    bool? inputResult, outputResult;
                    BlockSyntax? inputContractBlock, outputContractBlock;
                    var valueSyntax = IdentifierName( param.Name );

                    if ( this.ContractDirection is ContractDirection.Input or ContractDirection.Both )
                    {   
                        Invariant.Assert( param.RefKind is not RefKind.Out );
                        inputResult = this.TryExecuteTemplate( context, valueSyntax, out inputContractBlock );
                    }
                    else
                    {
                        inputResult = null;
                        inputContractBlock = null;
                    }

                    if ( this.ContractDirection is ContractDirection.Output or ContractDirection.Both )
                    {
                        Invariant.Assert( param.RefKind is not RefKind.None );
                        outputResult = this.TryExecuteTemplate( context, valueSyntax, out outputContractBlock );
                    }
                    else
                    {
                        outputResult = null;
                        outputContractBlock = null;
                    }

                    if (inputResult == false || outputResult == false)
                    {
                        return Array.Empty<InsertedStatement>();
                    }

                    var statements = new List<InsertedStatement>();

                    if (inputContractBlock != null)
                    {
                        statements.Add( new InsertedStatement( inputContractBlock, param, this, InsertedStatementKind.InputContract ) );
                    }

                    if ( outputContractBlock != null )
                    {
                        statements.Add( new InsertedStatement( outputContractBlock, param, this, InsertedStatementKind.OutputContract ) );
                    }

                    return statements;
                }

            default:
                throw new AssertionFailedException( $"Unsupported contract target: {this.ContractTarget}" );
        }
    }

    public override FormattableString ToDisplayString() => $"Add default contract to method '{this.TargetDeclaration}'";

    /*
    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var advice = (ContractAdvice) this.ParentAdvice;

        // Execute the templates.

        var hasOutputFilters = advice.Contracts.Any( f => f.AppliesTo( ContractDirection.Output ) );

        var iteratorInfo = this.OverriddenDeclaration.GetIteratorInfo();
        var asyncInfo = this.OverriddenDeclaration.GetAsyncInfo();

        // Determine whether the resulting method will be a state machine and the kind of template the transformation it will simulate.
        // If we use TemplateKind.Default, the proceed expression will be "buffered" and "awaited".
        // All contracts are normal methods and therefore there is no await or yield return in contract statements, so the only code that is "async" and "iterator" is
        // the servicing code generated by this method.
        // I.e.:
        //  * If there are any output contracts, use the default template. This will get us an expression that can be stored in a variable and passed on to the contract code.
        //    Injected method for async and iterators will remain async or iterator.
        //  * When there are no output contracts, use the appropriate template and return directly the result.
        //    Injected method will be always a regular method that returns the awaitable/iterable result directly.

        var (useStateMachine, templateKind) = (hasOutputFilters, asyncInfo, iteratorInfo) switch
        {
            (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerable or EnumerableKind.UntypedIEnumerable }) => (
                false, TemplateKind.IEnumerable),
            (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator }) => (
                false, TemplateKind.IEnumerator),
            (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerable }) => (false, TemplateKind.IAsyncEnumerable),
            (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerator }) => (false, TemplateKind.IAsyncEnumerator),
            (false, { IsAsync: true }, _) when this.OverriddenDeclaration.ReturnType.Is( SpecialType.Void ) => (true, TemplateKind.Default),
            (false, { IsAsync: true }, _) => (false, TemplateKind.Async),
            (true, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerable }) => (true, TemplateKind.IAsyncEnumerable),
            (true, { IsAsync: true }, _) => (true, TemplateKind.Default),
            _ => (false, TemplateKind.Default),
        };

        string? returnValueName;
        string? contractInputName;

        var isPartialWithContracts = this.OverriddenDeclaration.IsPartial;

        if ( !hasOutputFilters && !isPartialWithContracts )
        {
            return Array.Empty<InjectedMember>();
        }

        // Get output filter statements.
        if ( !this.OverriddenDeclaration.ReturnType.Is( SpecialType.Void ) )
        {
            // We need to store the return value before running filters.
            returnValueName = context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "returnValue" );

            if ( iteratorInfo is { EnumerableKind: EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator or EnumerableKind.IAsyncEnumerator } )
            {
                // For enumerator and async enumerator, filters need to get the input in a different variable.
                contractInputName = context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ).GetUniqueIdentifier( "contractEnumerator" );
            }
            else
            {
                // Otherwise, the stored return value is used.
                contractInputName = returnValueName;
            }
        }
        else
        {
            // There are filters on ref/out parameters.
            if ( this.OverriddenDeclaration.IsAsync )
            {
                throw new AssertionFailedException( $"{this.OverriddenDeclaration} is async, does not return anything, but has output filters." );
            }

            returnValueName = null;
            contractInputName = null;
        }

        if ( !advice.TryExecuteTemplates(
                this.OverriddenDeclaration,
                context,
                ContractDirection.Output,
                contractInputName,
                null,
                out var outputFilterBodies ) )
        {
            return Array.Empty<InjectedMember>();
        }

        // Get the proceed expression.
        var proceedExpression = this.CreateProceedExpression( context, templateKind )
            .ToExpressionSyntax( new( context.Compilation, context.SyntaxGenerationContext ) );

        var statements = new List<StatementSyntax>();

        // There are output filters.
        if ( returnValueName != null )
        {
            // var returnValue = <proceed>;
            statements.Add(
                LocalDeclarationStatement(
                    VariableDeclaration( SyntaxFactoryEx.VarIdentifier() )
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator( Identifier( default, returnValueName, new( ElasticSpace ) ) )
                                    .WithInitializer( EqualsValueClause( proceedExpression ) ) ) ) ) );

            if ( returnValueName != contractInputName )
            {
                // var contractInput = returnValue;
                statements.Add(
                    LocalDeclarationStatement(
                        VariableDeclaration( SyntaxFactoryEx.VarIdentifier() )
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator( Identifier( default, contractInputName.AssertNotNull(), new( ElasticSpace ) ) )
                                        .WithInitializer( EqualsValueClause( IdentifierName( returnValueName ) ) ) ) ) ) );
            }
        }
        else
        {
            // <proceed>;
            statements.Add( ExpressionStatement( proceedExpression ) );
        }

        if ( outputFilterBodies != null )
        {
            if ( returnValueName != contractInputName )
            {
                // Enumerator variable need to be reset after every return value contract.
                var first = true;

                foreach ( var outputFilterBody in outputFilterBodies )
                {
                    if ( !first )
                    {
                        statements.Add(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName( contractInputName.AssertNotNull() ),
                                    Token( TriviaList( ElasticSpace ), SyntaxKind.EqualsToken, TriviaList( ElasticSpace ) ),
                                    IdentifierName( returnValueName.AssertNotNull() ) ) ) );
                    }
                    else
                    {
                        first = false;
                    }

                    statements.Add( outputFilterBody );
                }
            }
            else
            {
                statements.AddRange( outputFilterBodies );
            }
        }

        if ( returnValueName != null )
        {
            if ( templateKind is TemplateKind.IEnumerable or TemplateKind.IAsyncEnumerable )
            {
                CreateEnumerableEpilogue( IdentifierName( returnValueName ) );
            }
            else if ( iteratorInfo.EnumerableKind is EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator
                     or EnumerableKind.IAsyncEnumerator )
            {
                CreateEnumeratorEpilogue( IdentifierName( returnValueName ) );
            }
            else
            {
                statements.Add(
                    ReturnStatement(
                        Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                        IdentifierName( returnValueName ),
                        Token( SyntaxKind.SemicolonToken ) ) );
            }
        }

        return this.GetInjectedMembersImpl( context, SyntaxFactoryEx.FormattedBlock( statements ), useStateMachine );

        void CreateEnumerableEpilogue( ExpressionSyntax enumerableExpression )
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
                    SyntaxFactoryEx.VarIdentifier(),
                    Identifier( returnItemName ),
                    Token( TriviaList( ElasticSpace ), SyntaxKind.InKeyword, TriviaList( ElasticSpace ) ),
                    enumerableExpression,
                    Token( SyntaxKind.CloseParenToken ),
                    Block(
                        YieldStatement(
                            SyntaxKind.YieldReturnStatement,
                            Token( TriviaList(), SyntaxKind.YieldKeyword, TriviaList( ElasticSpace ) ),
                            Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                            IdentifierName( returnItemName ),
                            Token( SyntaxKind.SemicolonToken ) ) ) ) );
        }

        void CreateEnumeratorEpilogue( ExpressionSyntax enumeratorExpression )
        {
            ExpressionSyntax moveNextExpression =
                this.OverriddenDeclaration.IsAsync
                    ? AwaitExpression(
                        Token( TriviaList(), SyntaxKind.AwaitKeyword, TriviaList( ElasticSpace ) ),
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                enumeratorExpression,
                                IdentifierName( "MoveNextAsync" ) ) ) )
                    : InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            enumeratorExpression,
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
                                enumeratorExpression,
                                IdentifierName( "Current" ) ),
                            Token( SyntaxKind.SemicolonToken ) ) ) ) );
        }
    }
    */
}