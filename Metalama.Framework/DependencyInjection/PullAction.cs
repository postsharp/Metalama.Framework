// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Represents a way to pull a field or property.
/// </summary>
[CompileTime]
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

    /// <summary>
    /// Gets a <see cref="PullAction"/> that means that the dependency has to be set to its default value.
    /// </summary>
    public static PullAction None => new( DependencyPullStrategyKind.DoNotPull );

    /// <summary>
    /// Creates a <see cref="PullAction"/> that means that the dependency should be pulled from an existing constructor parameter.
    /// </summary>
    public static PullAction UseExistingParameter( IParameter parameter ) => UseExpression( ExpressionFactory.Parse( parameter.Name ) );

    /// <summary>
    /// Creates a <see cref="PullAction"/> that means that the dependency should be pulled from a new parameter of the calling constructor.
    /// </summary>
    /// <param name="parameterName">Name of the new parameter.</param>
    /// <param name="parameterType">Type of the new parameter.</param>
    /// <param name="assignmentExpression">The right side of the statement that assigns the field or property dependency to the parameter.
    /// This value is optional. It is ignored if the dependency is a parameter (i.e. in the <see cref="IPullStrategy.PullParameter"/> method).
    /// When the dependency is a field or property, the default value is <paramref name="parameterName"/>.</param>
    /// <param name="buildParameterAction">An optional action that receives an <see cref="IParameterBuilder"/> and can add custom attributes to it.</param>
    /// <returns></returns>
    public static PullAction IntroduceParameterAndPull(
        string parameterName,
        IType parameterType,
        IExpression? assignmentExpression = null,
        Action<IParameterBuilder>? buildParameterAction = null )
        => new( DependencyPullStrategyKind.AppendParameterAndPull, buildParameterAction, assignmentExpression, parameterName, parameterType );

    /// <summary>
    /// Creates a <see cref="PullAction"/> that means that the dependency should be assigned to a given expression.
    /// </summary>
    public static PullAction UseExpression( IExpression expression )
        => new( DependencyPullStrategyKind.UseExistingParameter, assignmentExpression: expression );
}