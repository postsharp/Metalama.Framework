// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advices;
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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal abstract class FieldOrPropertyBuilder : MemberBuilder, IFieldOrPropertyBuilder
    {
        public abstract IType Type { get; set; }

        IMethod? IFieldOrProperty.GetMethod => this.GetMethod;

        public abstract IMethodBuilder? GetMethod { get; }

        IMethod? IFieldOrProperty.SetMethod => this.SetMethod;

        public abstract IMethodBuilder? SetMethod { get; }

        public abstract IExpression? InitializerExpression { get; set; }

        public abstract Writeability Writeability { get; set; }

        public abstract bool IsAutoPropertyOrField { get; }

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.FieldOrPropertyInvokers;

        // TODO: Having the renamed property is ugly, but the interface needs to be implemented and the abstract method cannot be overridden and hidden in the derived class at the same time in C#.
        protected abstract IInvokerFactory<IFieldOrPropertyInvoker> FieldOrPropertyInvokers { get; }

        public abstract IEnumerable<IMethod> Accessors { get; }

        protected FieldOrPropertyBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice, declaringType, name ) { }

        internal bool GetInitializerExpressionOrMethod<T>( 
            in MemberIntroductionContext context,
            IExpression? initializerExpression,
            TemplateMember<T> initializerTemplate,
            out ExpressionSyntax? initializerExpressionSyntax, 
            out MethodDeclarationSyntax? initializerMethodSyntax )
            where T : class, IMember
        {
            if ( initializerExpression != null )
            {
                // TODO: Error about the expression type?
                initializerMethodSyntax = null;
                initializerExpressionSyntax = ((IUserExpression) initializerExpression).ToRunTimeExpression().Syntax;
                return true;
            }
            else if ( initializerTemplate.IsNotNull )
            {
                initializerExpressionSyntax = null;

                if ( !this.TryExpandInitializerTemplate( context, initializerTemplate, out var initializerBlock ) )
                {
                    // Template expansion error.
                    initializerMethodSyntax = null;
                    initializerExpressionSyntax = null;
                    return false;
                }

                // If the initializer block contains only a single return statement, 
                if ( initializerBlock.Statements.Count == 1 && initializerBlock.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement )
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
                            context.SyntaxGenerator.Type( this.Type.GetSymbol() ),
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

        private bool TryExpandInitializerTemplate<T>(
            MemberIntroductionContext context,
            TemplateMember<T> initializerTemplate,
            [NotNullWhen( true )] out BlockSyntax? expression )
            where T : class, IMember
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

                var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( initializerTemplate.Declaration! );

                return templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out expression );
            }
        }

        public abstract IMethod? GetAccessor( MethodKind methodKind );

        public abstract FieldOrPropertyInfo ToFieldOrPropertyInfo();
    }
}