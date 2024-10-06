// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Contracts;

internal sealed class ContractPropertyTransformation : ContractBaseTransformation
{
    private new IRef<IProperty> TargetMember => (IRef<IProperty>) base.TargetMember;

    public ContractPropertyTransformation(
        Advice advice,
        IRef<IProperty> targetProperty,
        ContractDirection contractDirection,
        TemplateMember<IMethod> template,
        IObjectReader templateArguments,
        IObjectReader tags,
        TemplateProvider templateProvider ) : base(
        advice,
        targetProperty,
        targetProperty,
        contractDirection,
        template,
        templateProvider,
        templateArguments,
        tags ) { }

    public override IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        Invariant.Assert( this.ContractTarget.Equals( this.TargetMember ) );

        var targetMember = this.TargetMember.GetTarget( context.Compilation );

        Invariant.Assert( this.ContractDirection is ContractDirection.Output or ContractDirection.Input or ContractDirection.Both );

        bool? inputResult, outputResult;
        BlockSyntax? inputContractBlock, outputContractBlock;

        if ( this.ContractDirection is ContractDirection.Input or ContractDirection.Both )
        {
            Invariant.Assert( targetMember.SetMethod is not null );

            inputResult = this.TryExecuteTemplate( context, IdentifierName( "value" ), targetMember.Type, out inputContractBlock );
        }
        else
        {
            inputResult = null;
            inputContractBlock = null;
        }

        if ( this.ContractDirection is ContractDirection.Output or ContractDirection.Both )
        {
            Invariant.Assert( targetMember.GetMethod is not null );

            var returnVariableName = context.GetReturnValueVariableName();
            outputResult = this.TryExecuteTemplate( context, IdentifierName( returnVariableName ), targetMember.Type, out outputContractBlock );
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
                    targetMember.SetMethod.AssertNotNull().Parameters[0],
                    this,
                    InsertedStatementKind.InputContract ) );
        }

        if ( outputContractBlock != null )
        {
            statements.Add(
                new InsertedStatement(
                    outputContractBlock,
                    targetMember.GetMethod.AssertNotNull().ReturnParameter,
                    this,
                    InsertedStatementKind.OutputContract ) );
        }

        return statements;
    }

    public override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Add contract to property '{this.TargetMember.GetTarget( compilation ).ToDisplayString( CodeDisplayFormat.MinimallyQualified )}'";
}