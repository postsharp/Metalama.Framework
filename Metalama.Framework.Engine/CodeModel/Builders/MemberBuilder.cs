// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal abstract class MemberBuilder : MemberOrNamedTypeBuilder, IMemberBuilder, IMemberImpl
    {
        private bool _isVirtual;
        private bool _isAsync;
        private bool _isOverride;

        protected MemberBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice, declaringType, name ) { }

        public abstract bool IsImplicit { get; }

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public override string ToString() => this.DeclaringType + "." + this.Name;

        public abstract bool IsExplicitInterfaceImplementation { get; }

        public bool IsVirtual
        {
            get => this._isVirtual;
            set
            {
                this.CheckNotFrozen();

                this._isVirtual = value;
            }
        }

        public bool IsAsync
        {
            get => this._isAsync;
            set
            {
                this.CheckNotFrozen();

                this._isAsync = value;
            }
        }

        public bool IsOverride
        {
            get => this._isOverride;
            set
            {
                this.CheckNotFrozen();

                this._isOverride = value;
            }
        }

        public override bool IsDesignTime => !this.IsOverride;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.DeclaringType.ToDisplayString( format, context ) + "." + this.Name;

        public abstract IMember? OverriddenMember { get; }

        public override bool CanBeInherited => this.IsVirtual && !this.IsSealed && ((IDeclarationImpl) this.DeclaringType).CanBeInherited;

        internal bool GetInitializerExpressionOrMethod<T>(
            in MemberIntroductionContext context,
            IType targetType,
            IExpression? initializerExpression,
            TemplateMember<T> initializerTemplate,
            IObjectReader tags,
            out ExpressionSyntax? initializerExpressionSyntax,
            out MethodDeclarationSyntax? initializerMethodSyntax )
            where T : class, IMember
        {
            if ( context is null )
            {
                throw new ArgumentNullException( nameof(context) );
            }

            if ( targetType is null )
            {
                throw new ArgumentNullException( nameof(targetType) );
            }

            if ( context.SyntaxGenerationContext.IsPartial && (initializerExpression != null || initializerTemplate.IsNotNull) )
            {
                // At design time when generating the partial code for source generators, we do not expand templates.
                // This may cause warnings in the constructor (because some fields will not be initialized)
                // but we will add that later. The main point is that we should not execute the template here.

                initializerMethodSyntax = null;
                initializerExpressionSyntax = null;

                return true;
            }

            if ( initializerExpression != null )
            {
                // TODO: Error about the expression type?
                initializerMethodSyntax = null;
                initializerExpressionSyntax = ((IUserExpression) initializerExpression).ToSyntax( context.SyntaxGenerationContext );

                return true;
            }
            else if ( initializerTemplate.IsNotNull )
            {
                initializerExpressionSyntax = null;

                if ( !this.TryExpandInitializerTemplate( context, initializerTemplate, tags, out var initializerBlock ) )
                {
                    // Template expansion error.
                    initializerMethodSyntax = null;
                    initializerExpressionSyntax = null;

                    return false;
                }

                // If the initializer block contains only a single return statement, 
                if ( initializerBlock.Statements.Count == 1
                     && initializerBlock.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement )
                {
                    initializerMethodSyntax = null;
                    initializerExpressionSyntax = returnStatement.Expression;

                    return true;
                }

                var initializerName = context.IntroductionNameProvider.GetInitializerName( this.DeclaringType, this.ParentAdvice.AspectLayerId, this );

                if ( initializerBlock != null )
                {
                    initializerExpressionSyntax = InvocationExpression( IdentifierName( initializerName ) );

                    initializerMethodSyntax =
                        MethodDeclaration(
                            List<AttributeListSyntax>(),
                            TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) ),
                            context.SyntaxGenerator.Type( targetType.GetSymbol() ),
                            null,
                            Identifier( initializerName ),
                            null,
                            ParameterList(),
                            List<TypeParameterConstraintClauseSyntax>(),
                            initializerBlock,
                            null );

                    return true;
                }
                else
                {
                    initializerMethodSyntax = null;

                    return true;
                }
            }
            else
            {
                initializerMethodSyntax = null;
                initializerExpressionSyntax = null;

                return true;
            }
        }

        protected SyntaxToken GetCleanName()
        {
            return
                Identifier(
                    this.IsExplicitInterfaceImplementation
                        ? this.Name.Split( '.' ).Last()
                        : this.Name );
        }

        private bool TryExpandInitializerTemplate<T>(
            MemberIntroductionContext context,
            TemplateMember<T> initializerTemplate,
            IObjectReader tags,
            [NotNullWhen( true )] out BlockSyntax? expression )
            where T : class, IMember
        {
            var metaApi = MetaApi.ForInitializer(
                this,
                new MetaApiProperties(
                    this.ParentAdvice.SourceCompilation,
                    context.DiagnosticSink,
                    initializerTemplate.Cast(),
                    tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                context.LexicalScopeProvider.GetLexicalScope( this ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                default,
                null,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( initializerTemplate.Declaration! );

            return templateDriver.TryExpandDeclaration( expansionContext, Array.Empty<object>(), out expression );
        }
    }
}