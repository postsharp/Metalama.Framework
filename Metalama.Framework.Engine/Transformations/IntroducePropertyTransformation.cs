// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations;

internal class IntroducePropertyTransformation : IntroduceMemberTransformation<PropertyBuilder>
{
    public IntroducePropertyTransformation( Advice advice, PropertyBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context )
    {
        var propertyBuilder = this.IntroducedDeclaration;
        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

        // TODO: What if non-auto property has the initializer template?

        // If template fails to expand, we will still generate the field, albeit without the initializer.
        _ = propertyBuilder.GetPropertyInitializerExpressionOrMethod( this.ParentAdvice, context, out var initializerExpression, out var initializerMethod );

        // TODO: Indexers.
        var property =
            SyntaxFactory.PropertyDeclaration(
                propertyBuilder.GetAttributeLists( context ),
                propertyBuilder.GetSyntaxModifierList(),
                syntaxGenerator.Type( propertyBuilder.Type.GetSymbol() ),
                propertyBuilder.ExplicitInterfaceImplementations.Count > 0
                    ? SyntaxFactory.ExplicitInterfaceSpecifier(
                        (NameSyntax) syntaxGenerator.Type( propertyBuilder.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                    : null,
                this.GetCleanName(),
                GenerateAccessorList(),
                null,
                initializerExpression != null
                    ? SyntaxFactory.EqualsValueClause( initializerExpression )
                    : null,
                initializerExpression != null
                    ? SyntaxFactory.Token( SyntaxKind.SemicolonToken )
                    : default );

        var introducedProperty = new IntroducedMember(
            this,
            property,
            this.ParentAdvice.AspectLayerId,
            IntroducedMemberSemantic.Introduction,
            propertyBuilder );

        var introducedInitializerMethod =
            initializerMethod != null
                ? new IntroducedMember(
                    this,
                    initializerMethod,
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.InitializerMethod,
                    propertyBuilder )
                : null;

        if ( introducedInitializerMethod != null )
        {
            return new[] { introducedProperty, introducedInitializerMethod };
        }
        else
        {
            return new[] { introducedProperty };
        }

        AccessorListSyntax GenerateAccessorList()
        {
            switch (propertyBuilder.IsAutoPropertyOrField, propertyBuilder.Writeability, propertyBuilder.GetMethod, propertyBuilder.SetMethod)
            {
                // Properties with both accessors.
                case (false, _, not null, not null):
                // Writeable fields.
                case (true, Writeability.All, { IsImplicitlyDeclared: true }, { IsImplicitlyDeclared: true }):
                // Auto-properties with both accessors.
                case (true, Writeability.All or Writeability.InitOnly, { IsImplicitlyDeclared: false }, { IsImplicitlyDeclared: _ }):
                    return SyntaxFactory.AccessorList( SyntaxFactory.List( new[] { GenerateGetAccessor(), GenerateSetAccessor() } ) );

                // Init only fields.
                case (true, Writeability.InitOnly, { IsImplicitlyDeclared: true }, { IsImplicitlyDeclared: true }):
                    return SyntaxFactory.AccessorList( SyntaxFactory.List( new[] { GenerateGetAccessor(), GenerateSetAccessor() } ) );

                // Properties with only get accessor.
                case (false, _, not null, null):
                // Read only fields or get-only auto properties.
                case (true, Writeability.ConstructorOnly, { }, { IsImplicitlyDeclared: true }):
                    return SyntaxFactory.AccessorList( SyntaxFactory.List( new[] { GenerateGetAccessor() } ) );

                // Properties with only set accessor.
                case (_, _, null, not null):
                    return SyntaxFactory.AccessorList( SyntaxFactory.List( new[] { GenerateSetAccessor() } ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        AccessorDeclarationSyntax GenerateGetAccessor()
        {
            var tokens = new List<SyntaxToken>();

            if ( propertyBuilder.GetMethod!.Accessibility != propertyBuilder.Accessibility )
            {
                propertyBuilder.GetMethod.Accessibility.AddTokens( tokens );
            }

            return
                SyntaxFactory.AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        propertyBuilder.GetAttributeLists( context, propertyBuilder.GetMethod ),
                        SyntaxFactory.TokenList( tokens ),
                        SyntaxFactory.Token( SyntaxKind.GetKeyword ),
                        propertyBuilder.IsAutoPropertyOrField
                            ? null
                            : SyntaxFactory.Block(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( SyntaxFactory.Whitespace( " " ) ),
                                    SyntaxFactory.DefaultExpression( syntaxGenerator.Type( propertyBuilder.Type.GetSymbol() ) ),
                                    SyntaxFactory.Token( SyntaxKind.SemicolonToken ) ) ),
                        null,
                        propertyBuilder.IsAutoPropertyOrField ? SyntaxFactory.Token( SyntaxKind.SemicolonToken ) : default )
                    .NormalizeWhitespace();
        }

        AccessorDeclarationSyntax GenerateSetAccessor()
        {
            var tokens = new List<SyntaxToken>();

            if ( propertyBuilder.SetMethod!.Accessibility != propertyBuilder.Accessibility )
            {
                propertyBuilder.SetMethod.Accessibility.AddTokens( tokens );
            }

            return
                SyntaxFactory.AccessorDeclaration(
                    propertyBuilder.HasInitOnlySetter ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
                    propertyBuilder.GetAttributeLists( context, propertyBuilder.SetMethod ),
                    SyntaxFactory.TokenList( tokens ),
                    propertyBuilder.HasInitOnlySetter ? SyntaxFactory.Token( SyntaxKind.InitKeyword ) : SyntaxFactory.Token( SyntaxKind.SetKeyword ),
                    propertyBuilder.IsAutoPropertyOrField
                        ? null
                        : SyntaxFactory.Block(),
                    null,
                    propertyBuilder.IsAutoPropertyOrField ? SyntaxFactory.Token( SyntaxKind.SemicolonToken ) : default );
        }
    }
}