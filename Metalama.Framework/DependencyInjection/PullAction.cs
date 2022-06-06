// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.DependencyInjection;

public readonly struct PullAction
{
    internal DependencyPullStrategyKind Kind { get; }

    internal IExpression? AssignmentExpression { get; }

    internal IType? ParameterType { get; }

    internal string? ParameterName { get; }

    internal Action<IParameterBuilder>? BuildParameterAction { get; }

    private PullAction(
        DependencyPullStrategyKind kind,
        Action<IParameterBuilder>? buildParameterAction = null,
        IExpression? assignmentExpression = null,
        string? parameterName = null,
        IType? parameterType = null )
    {
        this.Kind = kind;
        this.BuildParameterAction = buildParameterAction;
        this.AssignmentExpression = assignmentExpression;
        this.ParameterType = parameterType;
        this.ParameterName = parameterName;
    }

    public static PullAction None => new( DependencyPullStrategyKind.DoNotPull );

    public static PullAction UseExistingParameter( IParameter parameter ) => UseExpression( ExpressionFactory.Parse( parameter.Name ) );

    public static PullAction IntroduceParameterAndPull(
        string parameterName,
        IType parameterType,
        IExpression? assignmentExpression = null,
        Action<IParameterBuilder>? buildParameterAction = null )
        => new( DependencyPullStrategyKind.AppendParameterAndPull, buildParameterAction, assignmentExpression, parameterName, parameterType );

    public static PullAction UseExpression( IExpression expression )
        => new( DependencyPullStrategyKind.UseExistingParameter, assignmentExpression: expression );
}