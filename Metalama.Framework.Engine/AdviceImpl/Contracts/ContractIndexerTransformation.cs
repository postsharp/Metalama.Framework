// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Contracts;

internal sealed class ContractIndexerTransformation : ContractBaseTransformation
{
    private new IIndexer TargetMember => (IIndexer) base.TargetMember;

    public ContractIndexerTransformation(
        Advice advice,
        IIndexer targetIndexer,
        IParameter? indexerParameter,
        ContractDirection contractDirection,
        TemplateMember<IMethod> template,
        IObjectReader templateArguments,
        IObjectReader tags ) : base(
        advice,
        targetIndexer,
        (IDeclaration?) indexerParameter ?? targetIndexer,
        contractDirection,
        template,
        templateArguments,
        tags ) { }

    public override IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        switch ( this.ContractTarget )
        {
            case IIndexer:
                {
                    Invariant.Assert( ReferenceEquals( this.ContractTarget, this.TargetMember ) );
                    Invariant.Assert( this.ContractDirection is ContractDirection.Output or ContractDirection.Input or ContractDirection.Both );

                    bool? inputResult, outputResult;
                    BlockSyntax? inputContractBlock, outputContractBlock;

                    if ( this.ContractDirection is ContractDirection.Input or ContractDirection.Both )
                    {
                        Invariant.Assert( this.TargetMember.SetMethod is not null );

                        inputResult = this.TryExecuteTemplate( context, IdentifierName( "value" ), this.TargetMember.Type, out inputContractBlock );
                    }
                    else
                    {
                        inputResult = null;
                        inputContractBlock = null;
                    }

                    if ( this.ContractDirection is ContractDirection.Output or ContractDirection.Both )
                    {
                        Invariant.Assert( this.TargetMember.GetMethod is not null );

                        var returnVariableName = context.GetReturnValueVariableName();

                        outputResult = this.TryExecuteTemplate(
                            context,
                            IdentifierName( returnVariableName ),
                            this.TargetMember.Type,
                            out outputContractBlock );
                    }
                    else
                    {
                        outputResult = null;
                        outputContractBlock = null;
                    }

                    if ( inputResult == false || outputResult == false )
                    {
                        return Array.Empty<InsertedStatement>();
                    }

                    var statements = new List<InsertedStatement>();

                    if ( inputContractBlock != null )
                    {
                        statements.Add(
                            new InsertedStatement(
                                inputContractBlock,
                                this.TargetMember.SetMethod.AssertNotNull().Parameters[^1],
                                this,
                                InsertedStatementKind.InputContract ) );
                    }

                    if ( outputContractBlock != null )
                    {
                        statements.Add(
                            new InsertedStatement(
                                outputContractBlock,
                                this.TargetMember.GetMethod.AssertNotNull().ReturnParameter,
                                this,
                                InsertedStatementKind.OutputContract ) );
                    }

                    return statements;
                }

            case IParameter parameter:
                {
                    Invariant.Assert( this.ContractDirection is ContractDirection.Output or ContractDirection.Input or ContractDirection.Both );

                    bool? inputResult, outputResult;
                    BlockSyntax? inputContractBlock, outputContractBlock;
                    var valueSyntax = IdentifierName( parameter.Name );

                    if ( this.ContractDirection is ContractDirection.Input or ContractDirection.Both )
                    {
                        Invariant.Assert( parameter.RefKind is not RefKind.Out );
                        inputResult = this.TryExecuteTemplate( context, valueSyntax, parameter.Type, out inputContractBlock );
                    }
                    else
                    {
                        inputResult = null;
                        inputContractBlock = null;
                    }

                    if ( this.ContractDirection is ContractDirection.Output or ContractDirection.Both )
                    {
                        Invariant.Assert( parameter.RefKind is not RefKind.None );
                        outputResult = this.TryExecuteTemplate( context, valueSyntax, parameter.Type, out outputContractBlock );
                    }
                    else
                    {
                        outputResult = null;
                        outputContractBlock = null;
                    }

                    if ( inputResult == false || outputResult == false )
                    {
                        return Array.Empty<InsertedStatement>();
                    }

                    var statements = new List<InsertedStatement>();

                    if ( inputContractBlock != null )
                    {
                        statements.Add( new InsertedStatement( inputContractBlock, parameter, this, InsertedStatementKind.InputContract ) );
                    }

                    if ( outputContractBlock != null )
                    {
                        statements.Add( new InsertedStatement( outputContractBlock, parameter, this, InsertedStatementKind.OutputContract ) );
                    }

                    return statements;
                }

            default:
                throw new AssertionFailedException( $"Unsupported contract target: {this.ContractTarget}" );
        }
    }

    public override FormattableString ToDisplayString()
        => $"Add default contract to indexer '{this.TargetDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified )}'";
}