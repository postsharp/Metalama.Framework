// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class FieldBuilder : MemberBuilder, IFieldBuilder, IFieldImpl
    {
        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public IType Type { get; set; }

        [Memo]
        public IMethod? GetMethod => new AccessorBuilder( this, MethodKind.PropertyGet );

        [Memo]
        public IMethod? SetMethod => new AccessorBuilder( this, MethodKind.PropertySet );

        public override bool IsExplicitInterfaceImplementation => false;

        public override IMember? OverriddenMember => null;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ), false );

        public Writeability Writeability { get; set; }

        Writeability IFieldOrProperty.Writeability => this.Writeability;

        public bool IsAutoPropertyOrField => true;

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public IExpression? InitializerExpression { get; set; }

        public TemplateMember<IField> InitializerTemplate { get; set; }

        public FieldBuilder( Advice parentAdvice, INamedType targetType, string name )
            : base( parentAdvice, targetType, name )
        {
            this.Type = this.Compilation.Factory.GetSpecialType( SpecialType.Object );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            ExpressionSyntax? initializerExpression;
            BlockSyntax? initializerBlock;

            if ( this.InitializerExpression != null )
            {
                // TODO: Error about the expression type?
                initializerBlock = null;
                initializerExpression = ((IUserExpression) this.InitializerExpression).ToRunTimeExpression().Syntax;
            }
            else if ( this.InitializerTemplate.IsNotNull )
            {
                initializerExpression = null;
                if (!this.TryExpandInitializerTemplate( context, this.InitializerTemplate, out initializerBlock ) )
                {
                    // Template expansion error.
                    return Enumerable.Empty<IntroducedMember>();
                }

                // This is a bit out of place, but works in most cases and if not done here, it would get quite complex in the linker.
                if (initializerBlock.Statements.Count == 1 && initializerBlock.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement )
                {
                    initializerBlock = null;
                    initializerExpression = returnStatement.Expression;
                }
            }
            else
            {
                initializerBlock = null;
                initializerExpression = null;
            }

            MethodDeclarationSyntax? initializerMethod;
            var initializerName = context.IntroductionNameProvider.GetInitializerName( this.DeclaringType, this.ParentAdvice.AspectLayerId, this );

            if (initializerBlock != null)
            {
                initializerExpression = InvocationExpression( IdentifierName( initializerName ) );
                initializerMethod =
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) ),
                        context.SyntaxGenerator.Type( this.Type.GetSymbol() ),
                        null,
                        Identifier( initializerName ),
                        null,
                        ParameterList(),
                        List<TypeParameterConstraintClauseSyntax>(),
                        initializerBlock,
                        null );
            }
            else
            {
                initializerMethod = null;
            }

            var field =
                FieldDeclaration(
                    List<AttributeListSyntax>(), // TODO: Attributes.
                    this.GetSyntaxModifierList(),
                    VariableDeclaration(
                        syntaxGenerator.Type( this.Type.GetSymbol() ),
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier( this.Name ),
                                null,
                                initializerExpression != null
                                    ? EqualsValueClause( initializerExpression! )
                                    : null ) ) ) );

            if ( initializerMethod != null )
            {
                return new[] 
                {
                    new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ),
                    new IntroducedMember( this, initializerMethod, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.InitializerMethod, this ),
                };
            }
            else
            {
                return new[] { new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }
        }

        private bool TryExpandInitializerTemplate( 
            MemberIntroductionContext context, 
            TemplateMember<IField> initializerTemplate,
            [NotNullWhen( true )] out BlockSyntax? expression )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this ) )
            {
                var metaApi = MetaApi.ForFieldOrPropertyInitializer(
                    this,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        initializerTemplate.Cast(),
                        this.ParentAdvice.ReadOnlyTags,
                        this.ParentAdvice.AspectLayerId,
                        context.SyntaxGenerationContext,
                        this.ParentAdvice.Aspect,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.ParentAdvice.Aspect.Aspect,
                    metaApi,
                    this.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this ),
                    context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    default,
                    null,
                    this.ParentAdvice.AspectLayerId );

                var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.InitializerTemplate.Declaration! );

                return templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out expression );
            }
        }

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => null
            };

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                if ( this.GetMethod != null )
                {
                    yield return this.GetMethod;
                }

                if ( this.SetMethod != null )
                {
                    yield return this.SetMethod;
                }
            }
        }

        public FieldInfo ToFieldInfo() => throw new NotImplementedException();

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();
    }
}