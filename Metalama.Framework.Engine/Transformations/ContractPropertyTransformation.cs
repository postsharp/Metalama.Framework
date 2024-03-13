// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class ContractPropertyTransformation : ContractBaseTransformation
{
    public new IProperty TargetMember => (IProperty) base.TargetMember;

    public ContractPropertyTransformation(
        Advice advice,
        IProperty targetProperty,
        ContractDirection contractDirection,
        TemplateMember<IMethod> template,
        IObjectReader templateArguments,
        IObjectReader tags ) : base( advice, targetProperty, targetProperty, contractDirection, template, templateArguments, tags )
    {
    }

    public override IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        Invariant.Assert( this.ContractTarget == this.TargetMember );
        Invariant.Assert( this.ContractDirection is ContractDirection.Output or ContractDirection.Input or ContractDirection.Both );

        bool? inputResult, outputResult;
        BlockSyntax? inputContractBlock, outputContractBlock;

        if ( this.ContractDirection is ContractDirection.Input or ContractDirection.Both )
        {
            Invariant.Assert( this.TargetMember.SetMethod is not null );

            inputResult = this.TryExecuteTemplate( context, IdentifierName( "value" ), out inputContractBlock );
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
            outputResult = this.TryExecuteTemplate( context, IdentifierName( returnVariableName ), out outputContractBlock );
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
            statements.Add( new InsertedStatement( inputContractBlock, this.TargetMember.SetMethod.AssertNotNull().Parameters[0], this, InsertedStatementKind.InputContract ) );
        }

        if ( outputContractBlock != null )
        {
            statements.Add( new InsertedStatement( outputContractBlock, this.TargetMember.GetMethod.AssertNotNull().ReturnParameter, this, InsertedStatementKind.OutputContract ) );
        }

        return statements;
    }

    public override FormattableString ToDisplayString() => $"Add contract to property '{this.TargetMember.ToDisplayString( CodeDisplayFormat.MinimallyQualified )}'";
}