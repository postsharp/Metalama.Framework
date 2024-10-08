// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceFieldTransformation : IntroduceMemberTransformation<FieldBuilderData>
{
    public IntroduceFieldTransformation( AdviceInfo advice, FieldBuilderData introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;
        var fieldBuilder = this.BuilderData.ToRef().GetTarget( context.Compilation );

        // If template fails to expand, we will still generate the field, albeit without the initializer.
        _ = AdviceSyntaxGenerator.GetInitializerExpressionOrMethod(
            fieldBuilder,
            this.ParentAdvice,
            context,
            fieldBuilder.Type,
            fieldBuilder.InitializerExpression,
            this.BuilderData.InitializerTemplate,
            this.BuilderData.InitializerTags,
            out var initializerExpression,
            out var initializerMethod );

        // If we are introducing a field into a struct in C# 10, it must have an explicit default value.
        if ( initializerExpression == null
             && fieldBuilder.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct
             && context.SyntaxGenerationContext.RequiresStructFieldInitialization )
        {
            initializerExpression = SyntaxFactoryEx.Default;
        }

        var field =
            FieldDeclaration(
                AdviceSyntaxGenerator.GetAttributeLists( fieldBuilder, context ),
                fieldBuilder.GetSyntaxModifierList(),
                VariableDeclaration(
                    syntaxGenerator.Type( fieldBuilder.Type )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier( fieldBuilder.Name ),
                            null,
                            initializerExpression != null
                                ? EqualsValueClause( initializerExpression )
                                : null ) ) ) );

        if ( initializerMethod != null )
        {
            return
            [
                new InjectedMember( this, field, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() ),
                new InjectedMember(
                    this,
                    initializerMethod,
                    this.AspectLayerId,
                    InjectedMemberSemantic.InitializerMethod,
                    this.BuilderData.ToRef() )
            ];
        }
        else
        {
            return [new InjectedMember( this, field, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() )];
        }
    }
}