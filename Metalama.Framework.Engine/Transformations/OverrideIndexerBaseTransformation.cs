// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Metalama.Framework.Code.SpecialType;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal abstract class OverrideIndexerBaseTransformation : OverridePropertyOrIndexerTransformation
    {
        public new IIndexer OverriddenDeclaration => (IIndexer) base.OverriddenDeclaration;

        public OverrideIndexerBaseTransformation(
            Advice advice,
            IIndexer overriddenDeclaration,
            IObjectReader tags )
            : base( advice, overriddenDeclaration, tags ) { }

        protected IEnumerable<InjectedMember> GetInjectedMembersImpl(
            in MemberInjectionContext context,
            BlockSyntax? getAccessorBody,
            BlockSyntax? setAccessorBody )
        {
            var setAccessorDeclarationKind =
                this.OverriddenDeclaration.Writeability is Writeability.InitOnly or Writeability.ConstructorOnly
                    ? SyntaxKind.InitAccessorDeclaration
                    : SyntaxKind.SetAccessorDeclaration;

            var overrides = new[]
            {
                new InjectedMember(
                    this,
                    IndexerDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) ),
                        context.SyntaxGenerator.IndexerType( this.OverriddenDeclaration ).WithTrailingTrivia( Space ),
                        null,
                        Token( SyntaxKind.ThisKeyword ),
                        this.GetParameterList( context ),
                        AccessorList(
                            List(
                                new[]
                                    {
                                        getAccessorBody != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration,
                                                List<AttributeListSyntax>(),
                                                default,
                                                getAccessorBody )
                                            : null,
                                        setAccessorBody != null
                                            ? AccessorDeclaration(
                                                setAccessorDeclarationKind,
                                                List<AttributeListSyntax>(),
                                                default,
                                                setAccessorBody )
                                            : null
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) ),
                        null,
                        default ),
                    this.ParentAdvice.AspectLayerId,
                    InjectedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            return overrides;
        }

        private BracketedParameterListSyntax GetParameterList( MemberInjectionContext context )
        {
            var originalParameterList = context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration, context.Compilation );
            var overriddenByParameterType = context.InjectionNameProvider.GetOverriddenByType( this.ParentAdvice.Aspect, this.OverriddenDeclaration );

            return originalParameterList.WithAdditionalParameters( (overriddenByParameterType, "__linker_param") );
        }

        protected bool TryExpandAccessorTemplate(
            in MemberInjectionContext context,
            BoundTemplateMethod accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpression =
                this.CreateProceedDynamicExpression( context, accessor, accessorTemplate.Template.SelectedKind );

            var metaApi = MetaApi.ForFieldOrPropertyOrIndexer(
                this.OverriddenDeclaration,
                accessor,
                new MetaApiProperties(
                    this.ParentAdvice.SourceCompilation,
                    context.DiagnosticSink,
                    accessorTemplate.Template.Cast(),
                    this.Tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                context.LexicalScopeProvider.GetLexicalScope( accessor ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                accessorTemplate.Template,
                proceedExpression,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.Template.Declaration );

            return templateDriver.TryExpandDeclaration( expansionContext, accessorTemplate.TemplateArguments, out body );
        }

        protected BuiltUserExpression CreateProceedDynamicExpression( in MemberInjectionContext context, IMethod accessor, TemplateKind templateKind )
            => accessor.MethodKind switch
            {
                MethodKind.PropertyGet => ProceedHelper.CreateProceedDynamicExpression(
                    context.SyntaxGenerationContext,
                    this.CreateProceedGetExpression( context ),
                    templateKind,
                    this.OverriddenDeclaration.GetMethod.AssertNotNull() ),
                MethodKind.PropertySet => new BuiltUserExpression(
                    this.CreateProceedSetExpression( context ),
                    this.OverriddenDeclaration.Compilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void ) ),
                _ => throw new AssertionFailedException( $"Unexpected MethodKind for '{accessor}': {accessor.MethodKind}." )
            };

        protected override ExpressionSyntax CreateProceedGetExpression( in MemberInjectionContext context )
            => context.AspectReferenceSyntaxProvider.GetIndexerReference(
                this.ParentAdvice.AspectLayerId,
                this.OverriddenDeclaration,
                AspectReferenceTargetKind.PropertyGetAccessor,
                context.SyntaxGenerator );

        protected override ExpressionSyntax CreateProceedSetExpression( in MemberInjectionContext context )
            => AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                context.AspectReferenceSyntaxProvider.GetIndexerReference(
                    this.ParentAdvice.AspectLayerId,
                    this.OverriddenDeclaration,
                    AspectReferenceTargetKind.PropertySetAccessor,
                    context.SyntaxGenerator ),
                IdentifierName( "value" ) );
    }
}