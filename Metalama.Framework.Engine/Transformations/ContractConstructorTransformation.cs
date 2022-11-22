﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations;

internal class ContractConstructorTransformation : BaseTransformation, IInsertStatementTransformation
{
    public ContractConstructorTransformation( Advice advice, IConstructor constructor ) : base( advice )
    {
        this.TargetMember = constructor;
    }

    public IMember TargetMember { get; }

    public IMemberOrNamedType ContextDeclaration => this.TargetMember;

    public IEnumerable<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        var advice = (ContractAdvice) this.ParentAdvice;

        // Execute the templates.

        _ = advice.TryExecuteTemplates( this.TargetMember, context, ContractDirection.Input, null, out var inputFilterBodies );

        if ( inputFilterBodies == null )
        {
            return Array.Empty<InsertedStatement>();
        }
        else
        {
            return inputFilterBodies.SelectEnumerable( x => new InsertedStatement( x, this.TargetMember ) );
        }
    }

    public override IDeclaration TargetDeclaration => this.TargetMember;

    public override TransformationObservability Observability => TransformationObservability.None;

    public override TransformationKind TransformationKind => TransformationKind.InsertStatement;

    public override FormattableString ToDisplayString() => $"Add default contract to constructor '{this.TargetMember}'";
}