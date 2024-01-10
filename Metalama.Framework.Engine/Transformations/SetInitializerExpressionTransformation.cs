// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class SetInitializerExpressionTransformation : BaseTransformation, IMemberLevelTransformation
{
    public IMember TargetMember { get; }

    public ExpressionSyntax InitializerExpression { get; }

    public SetInitializerExpressionTransformation( Advice advice, IFieldOrProperty targetMember, ExpressionSyntax initializerExpression ) : base( advice )
    {
        this.TargetMember = targetMember;
        this.InitializerExpression = initializerExpression;
    }

    public override IDeclaration TargetDeclaration => this.TargetMember;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override TransformationKind TransformationKind => TransformationKind.InsertStatement;

    public override FormattableString ToDisplayString()
        => $"Set the initializer expression of the {this.TargetMember.DeclarationKind} '{this.TargetMember}' to '{this.InitializerExpression}'.";
}