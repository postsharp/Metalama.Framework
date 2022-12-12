﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// A transformation that appends an argument to the initializer call of a constructor.
/// </summary>
internal sealed class IntroduceConstructorInitializerArgumentTransformation : BaseTransformation, IMemberLevelTransformation
{
    public IConstructor Constructor { get; }

    IMember IMemberLevelTransformation.TargetMember => this.Constructor;

    public int ParameterIndex { get; }

    public ExpressionSyntax Value { get; }

    public IntroduceConstructorInitializerArgumentTransformation( Advice advice, IConstructor constructor, int parameterIndex, ExpressionSyntax value ) : base(
        advice )
    {
        this.Constructor = constructor;
        this.ParameterIndex = parameterIndex;
        this.Value = value;
    }

    public ArgumentSyntax ToSyntax()
        => SyntaxFactory.Argument( this.Value ).WithAdditionalAnnotations( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation );

    public override IDeclaration TargetDeclaration => this.Constructor;

    public override TransformationObservability Observability => TransformationObservability.None;

    public override TransformationKind TransformationKind => TransformationKind.InsertConstructorInitializerArgument;

    public override FormattableString ToDisplayString() => $"Introduce an argument to the initializer of constructor '{this.Constructor}'.";
}