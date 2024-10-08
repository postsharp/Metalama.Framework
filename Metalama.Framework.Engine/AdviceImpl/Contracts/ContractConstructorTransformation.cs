﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Contracts;

internal sealed class ContractConstructorTransformation : ContractBaseTransformation
{
    private readonly IFullRef<IConstructor> _targetConstructor;

    public ContractConstructorTransformation(
        Advice advice,
        IFullRef<IConstructor> targetConstructor,
        IRef<IParameter> contractTarget,
        ContractDirection contractDirection,
        TemplateMember<IMethod> template,
        IObjectReader templateArguments,
        IObjectReader tags,
        TemplateProvider templateProvider ) : base(
        advice,
        contractTarget,
        contractDirection,
        template,
        templateProvider,
        templateArguments,
        tags )
    {
        this._targetConstructor = targetConstructor;
    }

    public override IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        switch ( this.ContractTarget.GetTarget( context.Compilation ) )
        {
            case IParameter param:
                {
                    Invariant.Assert( this.ContractDirection is ContractDirection.Output or ContractDirection.Input or ContractDirection.Both );

                    bool? inputResult, outputResult;
                    BlockSyntax? inputContractBlock, outputContractBlock;
                    var valueSyntax = IdentifierName( param.Name );

                    if ( this.ContractDirection is ContractDirection.Input or ContractDirection.Both )
                    {
                        Invariant.Assert( param.RefKind is not RefKind.Out );
                        inputResult = this.TryExecuteTemplate( context, valueSyntax, param.Type, out inputContractBlock );
                    }
                    else
                    {
                        inputResult = null;
                        inputContractBlock = null;
                    }

                    if ( this.ContractDirection is ContractDirection.Output or ContractDirection.Both )
                    {
                        Invariant.Assert( param.RefKind is not RefKind.None );
                        outputResult = this.TryExecuteTemplate( context, valueSyntax, param.Type, out outputContractBlock );
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

    public override IFullRef<IMember> TargetMember => this._targetConstructor;

    protected override FormattableString ToDisplayString( CompilationModel compilation ) => $"Add default contract to constructor '{this.TargetMember}'";
}