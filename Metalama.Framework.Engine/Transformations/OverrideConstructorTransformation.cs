// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class OverrideConstructorTransformation : OverrideMemberTransformation
{
    private new IConstructor OverriddenDeclaration => (IConstructor) base.OverriddenDeclaration;

    private BoundTemplateMethod Template { get; }

    public OverrideConstructorTransformation(
        Advice advice,
        IConstructor overriddenDeclaration,
        BoundTemplateMethod template,
        IObjectReader tags )
        : base( advice, overriddenDeclaration, tags )
    {
        this.Template = template;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var proceedExpression = this.CreateProceedExpression( context );

        var metaApi = MetaApi.ForConstructor(
            this.OverriddenDeclaration,
            new MetaApiProperties(
                this.ParentAdvice.SourceCompilation,
                context.DiagnosticSink,
                this.Template.TemplateMember.Cast(),
                this.Tags,
                this.ParentAdvice.AspectLayerId,
                context.SyntaxGenerationContext,
                this.ParentAdvice.Aspect,
                context.ServiceProvider,
                MetaApiStaticity.Default ) );

        var expansionContext = new TemplateExpansionContext(
            context,
            this.ParentAdvice.TemplateInstance.TemplateProvider,
            metaApi,
            this.OverriddenDeclaration,
            this.Template,
            _ => proceedExpression,
            this.ParentAdvice.AspectLayerId );

        var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.Template.TemplateMember.Declaration );

        if ( !templateDriver.TryExpandDeclaration( expansionContext, this.Template.TemplateArguments, out var newMethodBody ) )
        {
            // Template expansion error.
            return Enumerable.Empty<InjectedMember>();
        }

        var modifiers =
            this.OverriddenDeclaration
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe )
                .Insert( 0, SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

        var syntax =
            this.OverriddenDeclaration.IsStatic
                ? (MemberDeclarationSyntax) MethodDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList( modifiers ),
                    PredefinedType( Token( SyntaxKind.VoidKeyword ) ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.ParentAdvice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    null,
                    ParameterList(),
                    List<TypeParameterConstraintClauseSyntax>(),
                    newMethodBody,
                    null )
                : ConstructorDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList( Token( TriviaList(), SyntaxKind.PrivateKeyword, TriviaList( ElasticSpace ) ) ),
                    Identifier( this.OverriddenDeclaration.DeclaringType.Name ),
                    this.GetParameterList( context ),
                    ConstructorInitializer(
                        SyntaxKind.ThisConstructorInitializer,
                        ArgumentList( SeparatedList( this.OverriddenDeclaration.Parameters.SelectAsArray( x => Argument( IdentifierName( x.Name ) ) ) ) ) ),
                    newMethodBody,
                    null );

        return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Override, this.OverriddenDeclaration ) };
    }

    private ParameterListSyntax GetParameterList( MemberInjectionContext context )
    {
        var originalParameterList = context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration, context.Compilation, true );
        var overriddenByParameterType = context.InjectionNameProvider.GetOverriddenByType( this.ParentAdvice.Aspect, this.OverriddenDeclaration );

        return originalParameterList.WithAdditionalParameters( (overriddenByParameterType, AspectReferenceSyntaxProvider.LinkerOverrideParamName) );
    }

    private SyntaxUserExpression CreateProceedExpression( MemberInjectionContext context )
        => new(
            this.OverriddenDeclaration.IsStatic
                ? context.AspectReferenceSyntaxProvider.GetStaticConstructorReference( this.ParentAdvice.AspectLayerId )
                : context.AspectReferenceSyntaxProvider.GetConstructorReference(
                    this.ParentAdvice.AspectLayerId,
                    this.OverriddenDeclaration,
                    context.SyntaxGenerator ),
            this.OverriddenDeclaration.GetCompilationModel().Cache.SystemVoidType );

    public override TransformationObservability Observability => TransformationObservability.None;
}