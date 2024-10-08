// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

/// <summary>
/// A transformation that appends an argument to the initializer call of a constructor.
/// </summary>
internal sealed class IntroduceConstructorInitializerArgumentTransformation : BaseSyntaxTreeTransformation, IMemberLevelTransformation
{
    private IRef<IConstructor> Constructor { get; }

    IRef<IMember> IMemberLevelTransformation.TargetMember => this.Constructor;

    public int ParameterIndex { get; }

    private ExpressionSyntax Value { get; }

    public IntroduceConstructorInitializerArgumentTransformation(
        Advice advice,
        IRef<IConstructor> constructor,
        int parameterIndex,
        ExpressionSyntax value ) : base( advice )
    {
        this.Constructor = constructor;
        this.ParameterIndex = parameterIndex;
        this.Value = value;
    }

    public ArgumentSyntax ToSyntax()
        => SyntaxFactory.Argument( this.Value ).WithAdditionalAnnotations( this.AspectInstance.AspectClass.GeneratedCodeAnnotation );

    public override IRef<IDeclaration> TargetDeclaration => this.Constructor;

    public override TransformationObservability Observability => TransformationObservability.None;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.InsertConstructorInitializerArgument;

    protected override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Introduce an argument to the initializer of constructor '{this.Constructor}'.";
}