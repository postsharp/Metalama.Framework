// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Finalizer override, which expands a template.
/// </summary>
internal sealed class OverrideFinalizerTransformation : OverrideMemberTransformation
{
    private BoundTemplateMethod BoundTemplate { get; }

    private new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

    public OverrideFinalizerTransformation( Advice advice, IMethod targetFinalizer, BoundTemplateMethod boundTemplate, IObjectReader tags )
        : base( advice, targetFinalizer, tags )
    {
        this.BoundTemplate = boundTemplate;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var proceedExpression = this.CreateProceedExpression( context );

        var metaApi = MetaApi.ForMethod(
            this.OverriddenDeclaration,
            new MetaApiProperties(
                this.ParentAdvice.SourceCompilation,
                context.DiagnosticSink,
                this.BoundTemplate.TemplateMember.Cast(),
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
            this.BoundTemplate,
            _ => proceedExpression,
            this.ParentAdvice.AspectLayerId );

        var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.BoundTemplate.TemplateMember.Declaration );

        if ( !templateDriver.TryExpandDeclaration( expansionContext, this.BoundTemplate.TemplateArguments, out var newMethodBody ) )
        {
            // Template expansion error.
            return Enumerable.Empty<InjectedMember>();
        }

        var modifiers = this.OverriddenDeclaration
            .GetSyntaxModifierList( ModifierCategories.Unsafe )
            .Insert( 0, SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

        var syntax =
            MethodDeclaration(
                List<AttributeListSyntax>(),
                TokenList( modifiers ),
                PredefinedType( SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.VoidKeyword ) ),
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
                null );

        return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Override, this.OverriddenDeclaration ) };
    }

    private SyntaxUserExpression CreateProceedExpression( MemberInjectionContext context )
        => new(
            context.AspectReferenceSyntaxProvider.GetFinalizerReference( this.ParentAdvice.AspectLayerId ),
            this.OverriddenDeclaration.GetCompilationModel().Cache.SystemVoidType );

    public override TransformationObservability Observability => TransformationObservability.None;
}