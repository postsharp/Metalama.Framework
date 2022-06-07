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