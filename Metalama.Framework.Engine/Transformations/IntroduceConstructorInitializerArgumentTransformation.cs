// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// A transformation that appends an argument to the initializer call of a constructor.
/// </summary>
internal class IntroduceConstructorInitializerArgumentTransformation : BaseTransformation, INonObservableTransformation, IMemberLevelTransformation
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
}