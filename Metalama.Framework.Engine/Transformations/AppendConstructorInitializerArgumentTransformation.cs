// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations;

internal class AppendParameterTransformation : IObservableTransformation, IMemberLevelTransformation
{
    public Advice Advice { get; }

    IDeclaration IObservableTransformation.ContainingDeclaration => this.TargetMember;

    public bool IsDesignTime => true;

    public SyntaxTree TargetSyntaxTree
        => (this.TargetMember.GetPrimaryDeclaration() ?? this.TargetMember.DeclaringType.GetPrimaryDeclaration().AssertNotNull()).SyntaxTree;

    public IMember TargetMember => this.Parameter.DeclaringMember;

    public IParameter Parameter { get; }

    public AppendParameterTransformation( Advice advice, IParameter parameter )
    {
        this.Advice = advice;
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

        if ( this.Parameter.DefaultValue.IsAssigned )
        {
            syntax = syntax.WithDefault(
                SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.Token( SyntaxKind.EqualsToken )
                        .WithLeadingTrivia( SyntaxFactory.ElasticSpace )
                        .WithTrailingTrivia( SyntaxFactory.ElasticSpace ),
                    syntaxGenerationContext.SyntaxGenerator.TypedConstant( this.Parameter.DefaultValue ) ) );
        }

        return syntax;
    }
}

/// <summary>
/// A transformation that appends an argument to the initializer call of a constructor.
/// </summary>
internal class AppendConstructorInitializerArgumentTransformation : INonObservableTransformation, IMemberLevelTransformation
{
    public IConstructor Constructor { get; }

    IMember IMemberLevelTransformation.TargetMember => this.Constructor;

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

    public SyntaxTree TargetSyntaxTree
        => (this.Constructor.GetPrimaryDeclaration() ?? this.Constructor.DeclaringType.GetPrimaryDeclaration().AssertNotNull()).SyntaxTree;

    public ArgumentSyntax ToSyntax()
        => SyntaxFactory.Argument( this.Value ).WithAdditionalAnnotations( this.Advice.Aspect.AspectClass.GeneratedCodeAnnotation );
}