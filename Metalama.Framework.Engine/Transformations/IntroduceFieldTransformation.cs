// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceFieldTransformation : IntroduceMemberTransformation<FieldBuilder>
{
    public IntroduceFieldTransformation( Advice advice, FieldBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;
        var fieldBuilder = this.IntroducedDeclaration;

        // If template fails to expand, we will still generate the field, albeit without the initializer.
        _ = fieldBuilder.GetInitializerExpressionOrMethod(
            this.ParentAdvice,
            context,
            fieldBuilder.Type,
            fieldBuilder.InitializerExpression,
            fieldBuilder.InitializerTemplate,
            fieldBuilder.InitializerTags,
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
                fieldBuilder.GetAttributeLists( context ),
                fieldBuilder.GetSyntaxModifierList(),
                VariableDeclaration(
                    syntaxGenerator.Type( fieldBuilder.Type.GetSymbol() ).WithTrailingTrivia( Space ),
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier( fieldBuilder.Name ),
                            null,
                            initializerExpression != null
                                ? EqualsValueClause( initializerExpression )
                                : null ) ) ) );

        if ( initializerMethod != null )
        {
            return new[]
            {
                new InjectedMember( this, field, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, fieldBuilder ),
                new InjectedMember(
                    this,
                    initializerMethod,
                    this.ParentAdvice.AspectLayerId,
                    InjectedMemberSemantic.InitializerMethod,
                    fieldBuilder )
            };
        }
        else
        {
            return new[] { new InjectedMember( this, field, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, fieldBuilder ) };
        }
    }
}