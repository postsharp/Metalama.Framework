// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceStaticConstructorTransformation : IntroduceMemberTransformation<ConstructorBuilderData>, IReplaceMemberTransformation
{
    public IntroduceStaticConstructorTransformation( AspectLayerInstance aspectLayerInstance, ConstructorBuilderData introducedDeclaration ) : base(
        aspectLayerInstance,
        introducedDeclaration )
    {
        Invariant.Assert( introducedDeclaration.IsStatic );
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var constructorBuilder = this.BuilderData.ToRef().GetTarget( context.FinalCompilation );

        var syntax =
            ConstructorDeclaration(
                AdviceSyntaxGenerator.GetAttributeLists( constructorBuilder, context ),
                TokenList( Token( TriviaList(), SyntaxKind.StaticKeyword, TriviaList( Space ) ) ),
                Identifier( constructorBuilder.DeclaringType.Name ),
                ParameterList(),
                null,
                context.SyntaxGenerator.FormattedBlock().WithGeneratedCodeAnnotation( this.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                null );

        return
        [
            new InjectedMember(
                this,
                syntax,
                this.AspectLayerId,
                InjectedMemberSemantic.Introduction,
                this.BuilderData.ToRef() )
        ];
    }

    public IRef<IMember>? ReplacedMember => this.BuilderData.ReplacedImplicitConstructor;

    public override InsertPosition InsertPosition => this.ReplacedMember?.ToInsertPosition() ?? this.BuilderData.InsertPosition;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    protected override FormattableString ToDisplayString( CompilationModel compilation ) => $"Introduce a static constructor into '{this.TargetDeclaration}'.";
}