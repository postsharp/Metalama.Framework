﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceParameterTransformation : BaseSyntaxTreeTransformation, IMemberLevelTransformation
{
    public IRef<IMember> TargetMember => this.Parameter.ContainingDeclaration.As<IMember>();

    public ParameterBuilderData Parameter { get; }

    public IntroduceParameterTransformation( AspectLayerInstance aspectLayerInstance, ParameterBuilderData parameter ) : base(
        aspectLayerInstance,
        parameter.ContainingDeclaration )
    {
        this.Parameter = parameter;
    }

    public ParameterSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
    {
        var syntax = SyntaxFactory.Parameter(
            default,
            default,
            syntaxGenerationContext.SyntaxGenerator.Type( this.Parameter.Type )
                .WithOptionalTrailingTrivia( SyntaxFactory.ElasticSpace, syntaxGenerationContext.Options ),
            SyntaxFactory.Identifier( this.Parameter.Name ),
            null );

        if ( this.Parameter.DefaultValue != null )
        {
            syntax = syntax.WithDefault(
                SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.Token(
                        new SyntaxTriviaList( SyntaxFactory.ElasticSpace ),
                        SyntaxKind.EqualsToken,
                        new SyntaxTriviaList( SyntaxFactory.ElasticSpace ) ),
                    syntaxGenerationContext.SyntaxGenerator.TypedConstant( this.Parameter.DefaultValue.Value ) ) );
        }

        return syntax;
    }

    public override IRef<IDeclaration> TargetDeclaration => this.TargetMember;

    public override TransformationObservability Observability => TransformationObservability.Always;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceParameter;

    protected override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Introduce parameter '{this.Parameter.Name}' into '{this.Parameter.ContainingDeclaration}'.";
}