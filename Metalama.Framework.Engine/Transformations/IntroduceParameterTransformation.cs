﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceParameterTransformation : BaseTransformation, IMemberLevelTransformation
{
    public IMember TargetMember => this.Parameter.DeclaringMember;

    public IParameter Parameter { get; }

    public IntroduceParameterTransformation( Advice advice, IParameter parameter ) : base( advice )
    {
        this.Parameter = parameter;
    }

    public ParameterSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
    {
        var syntax = SyntaxFactory.Parameter(
            default,
            default,
            syntaxGenerationContext.SyntaxGenerator.Type( this.Parameter.Type.GetSymbol() ).WithTrailingTrivia( SyntaxFactory.ElasticSpace ),
            SyntaxFactory.Identifier( this.Parameter.Name ),
            null );

        if ( this.Parameter.DefaultValue != null )
        {
            syntax = syntax.WithDefault(
                SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.Token( SyntaxKind.EqualsToken )
                        .WithLeadingTrivia( SyntaxFactory.ElasticSpace )
                        .WithTrailingTrivia( SyntaxFactory.ElasticSpace ),
                    syntaxGenerationContext.SyntaxGenerator.TypedConstant( this.Parameter.DefaultValue.Value ) ) );
        }

        return syntax;
    }

    public override IDeclaration TargetDeclaration => this.TargetMember;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override TransformationKind TransformationKind => TransformationKind.IntroduceParameter;

    public override FormattableString ToDisplayString() => $"Introduce the parameter '{this.Parameter}'.";
}