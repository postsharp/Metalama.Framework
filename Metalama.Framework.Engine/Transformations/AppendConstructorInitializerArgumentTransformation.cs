// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// A transformation that appends an argument to the initializer call of a constructor.
/// </summary>
internal class AppendConstructorInitializerArgumentTransformation : INonObservableTransformation, ISyntaxTreeTransformation
{
    public IConstructor Constructor { get; }

    public int ParameterIndex { get; }

    public Advice Advice { get; }

    public ExpressionSyntax Value { get; }

    public AppendConstructorInitializerArgumentTransformation( Advice advice, IConstructor constructor, int parameterIndex, ExpressionSyntax value )
    {
        this.Constructor = constructor;
        this.ParameterIndex = parameterIndex;
        this.Advice = advice;
        this.Value = value;
    }

    public SyntaxTree TargetSyntaxTree => (this.Constructor.GetPrimaryDeclaration() ?? this.Constructor.DeclaringType.GetPrimaryDeclaration().AssertNotNull()).SyntaxTree;
}