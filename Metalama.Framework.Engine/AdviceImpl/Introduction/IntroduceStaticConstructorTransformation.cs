// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceStaticConstructorTransformation : IntroduceMemberTransformation<ConstructorBuilder>, IReplaceMemberTransformation
{
    public IntroduceStaticConstructorTransformation( Advice advice, ConstructorBuilder introducedDeclaration ) : base( advice, introducedDeclaration )
    {
        Invariant.Assert( introducedDeclaration.IsStatic );

        var targetType = introducedDeclaration.DeclaringType;
        this.ReplacedMember = targetType.StaticConstructor;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var constructorBuilder = this.IntroducedDeclaration;

        var syntax =
            ConstructorDeclaration(
                constructorBuilder.GetAttributeLists( context ),
                TokenList( Token( TriviaList(), SyntaxKind.StaticKeyword, TriviaList( Space ) ) ),
                Identifier( constructorBuilder.DeclaringType.Name ),
                ParameterList(),
                null,
                context.SyntaxGenerator.FormattedBlock().WithGeneratedCodeAnnotation( this.ParentAdvice.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                null );

        return new[]
        {
            new InjectedMember(
                this,
                syntax,
                this.ParentAdvice.AspectLayerId,
                InjectedMemberSemantic.Introduction,
                constructorBuilder )
        };
    }

    public IMember? ReplacedMember { get; }

    public override InsertPosition InsertPosition => this.ReplacedMember?.ToInsertPosition() ?? this.IntroducedDeclaration.ToInsertPosition();

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override FormattableString ToDisplayString() => $"Introduce a static constructor into '{this.TargetDeclaration}'.";
}